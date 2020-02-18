using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;
using EnvDTE;

namespace JavaDocConverterExtension
{
    public class DocumentParser
    {
        // Define Visual Studio constants
        public int vsFindOptionsNone = 0;
        public int vsFindOptionsRegularExpression = 8;

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider _serviceProvider;

        public DocumentParser(Microsoft.VisualStudio.Shell.IAsyncServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref=""></exception>
        /// <see cref=""/>
        public async Task ParseDocumentAsync()
        {

            if (Microsoft.VisualStudio.Shell.ThreadHelper.CheckAccess())
            {
                await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var dte = await GetDTEAsync();
                var activeDocument = await GetActiveDocumentAsync();

                var selection = activeDocument.Selection as TextSelection;
                selection.StartOfDocument(false);
                selection.EndOfLine(true);

                var text = activeDocument.Selection as TextSelection;

                while (text.FindText(DocumentTag.CommentBase.JavaDocTag, vsFindOptionsNone))
                {
                    int lineIndex = 0;
                    int endLine = 0;
                    Boolean isFoundInnerElement = false;
                    int innerElementLine = 0;

                    lineIndex = selection.CurrentLine;
                    FindCommentEnd(ref text, out endLine);

                    // If the comment is not the same line
                    if (lineIndex <= endLine)
                    {
                        for (int i = lineIndex; i <= endLine; i++)
                        {
                            selection.GotoLine(i, true);

                            // if the first time found the inner element
                            if ((!isFoundInnerElement) && (IsInnerElement(selection.Text)))
                            {
                                innerElementLine = i;
                                isFoundInnerElement = true;
                            }
                            var source = selection.Text;

                            if (FindElement(source, DocumentTag.CommentBase))
                            {
                                source = ParseElement(source, DocumentTag.CommentBase);
                            }
                            else if (FindElement(source, DocumentTag.Param))
                            {
                                source = ParseElement(source, DocumentTag.Param);
                            }
                            else if (FindElement(source, DocumentTag.Return))
                            {
                                source = ParseElement(source, DocumentTag.Return);
                            }
                            else if (FindElement(source, DocumentTag.Throws))
                            {
                                source = ParseElement(source, DocumentTag.Throws);
                            }
                            else if (FindElement(source, DocumentTag.Exception))
                            {
                                source = ParseElement(source, DocumentTag.Exception);
                            }
                            else if (FindElement(source, DocumentTag.See))
                            {
                                source = ParseElement(source, DocumentTag.See);
                            }
                            else if (FindElement(source, DocumentTag.CommentBaseEnd))
                            {
                                source = ParseElement(source, DocumentTag.CommentBaseEnd);
                            }
                            else if (FindElement(source, DocumentTag.CommentBody))
                            {
                                source = ParseElement(source, DocumentTag.CommentBody);
                            }

                            selection.Text = "";
                            selection.Insert(source);
                        }

                        if (innerElementLine > 0)
                        {
                            selection.GotoLine(innerElementLine, true);
                            selection.LineDown(true, endLine - innerElementLine - 1);
                            dte.ExecuteCommand("Edit.MoveSelectedLinesDown");
                        }
                    }
                }

                dte.ActiveDocument.ReplaceText("<summary></summary>", "<summary>");
                dte.ActiveDocument.ReplaceText(@"name=""=""", @"name=""");
                dte.ActiveDocument.ReplaceText(@"cref=""cref=""", @"cref=""");
                dte.ActiveDocument.ReplaceText("</param></param>", "</param>");
                dte.ActiveDocument.ReplaceText("</typeparam>typeparam></typeparam>", "</typeparam>");
                dte.ActiveDocument.ReplaceText("</returns></returns>", "</returns>");
                dte.ActiveDocument.ReplaceText(@"</param>""", "</param>");
                dte.ActiveDocument.ReplaceText(@"</returns>""", "</returns>");
                dte.ActiveDocument.ReplaceText(@"</exception>""", "</exception>");
                dte.ActiveDocument.ReplaceText(@"</see>""/>", "</see>");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="element"></param>
        /// <param name="IsNamedElement"></param>
        /// <returns></returns>
        private String ParseElement(String source, DocumentTag element, Boolean IsNamedElement = false)
        {
            String[] line = source.Trim().Split(' ');

            if (element == DocumentTag.CommentBody)
            {
                if (line[0] == DocumentTag.CommentBody.JavaDocTag)
                {
                    line[0] = DocumentTag.CommentBody.CSharpDocTagBegin;
                }
            }
            else
            {
                if (line[0] == DocumentTag.CommentBody.JavaDocTag)
                {
                    line[0] = DocumentTag.CommentBody.CSharpDocTagBegin;
                }

                for (int i = 0; i < line.Length; i++)
                {
                    line[i] = line[i].Trim();
                    if (line[i] == element.JavaDocTag)
                    {
                        line[i] = element.CSharpDocTagBegin;
                        if (element.IsNamedElement)
                        {
                            if (i < line.Length)
                                line[i + 1] = line[i + 1] + element.CSharpDocTagBeginClose;
                        }
                        break;
                    }
                    if ((line[i] == "<p>") || (line[i] == "<P>"))
                        line[i] = "";
                }

            }
            var result = element.NeedCloseTag ? String.Join(" ", line) + element.CSharpDocTagEnd : String.Join(" ", line);

            result = result.Replace(element.CSharpDocTagBegin + " ", element.CSharpDocTagBegin);

            if (element.IsNamedElement)
            {
                result = result.Replace(@"""> ", @""">");
            }

            // Check if the param tag is for type parameter
            if (element == DocumentTag.Param)
            {
                if (result.Contains(@"<param name=""<"))
                {
                    result = result.Replace(@"<param name=""<", DocumentTag.TypeParam.CSharpDocTagBegin);
                    result = result.Replace(@">"">", @""">");
                    result = result.Replace(DocumentTag.Param.CSharpDocTagEnd, DocumentTag.TypeParam.CSharpDocTagEnd);
                }
            }

            return result;
        }

        private Boolean FindElement(String source, DocumentTag element)
        {
            return source.Contains(element.JavaDocTag);
        }

        private Boolean IsInnerElement(String source)
        {
            if (source.Contains(DocumentTag.Param.JavaDocTag))
                return true;
            if (source.Contains(DocumentTag.Return.JavaDocTag))
                return true;
            if (source.Contains(DocumentTag.Throws.JavaDocTag))
                return true;
            if (source.Contains(DocumentTag.Exception.JavaDocTag))
                return true;
            if (source.Contains(DocumentTag.See.JavaDocTag))
                return true;

            return false;
        }

        private Boolean IsCommentBegin(ref TextSelection selection, int lineIndex)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            selection.GotoLine(lineIndex, true);
            String text = selection.Text;
            return text.Contains(DocumentTag.CommentBase.JavaDocTag);
        }

        private Boolean IsCommentEnd(ref TextSelection selection, int lineIndex)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            selection.GotoLine(lineIndex, true);
            String text = selection.Text;
            return text.Contains(DocumentTag.CommentBaseEnd.JavaDocTag);
        }

        private Boolean FindCommentBegin(ref TextSelection selection, out int Index)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var result = selection.FindText(DocumentTag.CommentBase.JavaDocTag, vsFindOptionsNone);
            Index = selection.CurrentLine;

            return result;
        }

        private Boolean FindCommentEnd(ref TextSelection selection, out int Index)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var result = selection.FindText(DocumentTag.CommentBaseEnd.JavaDocTag, vsFindOptionsNone);
            Index = selection.CurrentLine;

            return result;
        }

        public async Task<Document> GetActiveDocumentAsync()
        {
            return (await GetDTEAsync()).ActiveDocument;
        }

        public async Task<EnvDTE80.DTE2> GetDTEAsync()
        {
            var service = _serviceProvider.GetServiceAsync(typeof(DTE));
            EnvDTE80.DTE2 applicationObject = await Task.Run(() => service)as EnvDTE80.DTE2;
            return applicationObject;
        }

        public async Task<string> GetSelectedTextAsync()
        {
            var service = _serviceProvider.GetServiceAsync(typeof(SVsTextManager));
            var textManager = await Task.Run(() => service)as IVsTextManager2;
            IVsTextView view;
            int result = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out view);

            view.GetSelection(out int startLine, out int startColumn, out int endLine, out int endColumn);//end could be before beginning
            var start = new TextViewPosition(startLine, startColumn);
            var end = new TextViewPosition(endLine, endColumn);

            view.GetSelectedText(out string selectedText);

            return selectedText;
        }
    }
}
