using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.TextManager.Interop;
using EnvDTE;

namespace JavaDocConverterExtension
{
    public struct TextViewSelection
    {
        public TextViewPosition StartPosition { get; set; }
        public TextViewPosition EndPosition { get; set; }
        public string Text { get; set; }

        public TextViewSelection(TextViewPosition a, TextViewPosition b, string text)
        {
            StartPosition = TextViewPosition.Min(a, b);
            EndPosition = TextViewPosition.Max(a, b);
            Text = text;
        }
    }

    public struct TextViewPosition
    {
        private readonly int _column;
        private readonly int _line;

        public TextViewPosition(int line, int column)
        {
            _line = line;
            _column = column;
        }

        public int Line { get { return _line; } }
        public int Column { get { return _column; } }


        public static bool operator <(TextViewPosition a, TextViewPosition b)
        {
            if (a.Line < b.Line)
            {
                return true;
            }
            else if (a.Line == b.Line)
            {
                return a.Column < b.Column;
            }
            else
            {
                return false;
            }
        }

        public static bool operator >(TextViewPosition a, TextViewPosition b)
        {
            if (a.Line > b.Line)
            {
                return true;
            }
            else if (a.Line == b.Line)
            {
                return a.Column > b.Column;
            }
            else
            {
                return false;
            }
        }

        public static TextViewPosition Min(TextViewPosition a, TextViewPosition b)
        {
            return a > b ? b : a;
        }

        public static TextViewPosition Max(TextViewPosition a, TextViewPosition b)
        {
            return a > b ? a : b;
        }
    }

    public static class TextViewSelectionUtility
    {

        public static async System.Threading.Tasks.Task<TextViewSelection> GetSelectionAsync(Microsoft.VisualStudio.Shell.IAsyncServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetServiceAsync(typeof(SVsTextManager));
            var textManager = await Task.Run(() => service)as IVsTextManager2;
            IVsTextView view;
            int result = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out view);

            view.GetSelection(out int startLine, out int startColumn, out int endLine, out int endColumn);//end could be before beginning
            var start = new TextViewPosition(startLine, startColumn);
            var end = new TextViewPosition(endLine, endColumn);

            view.GetSelectedText(out string selectedText);

            TextViewSelection selection = new TextViewSelection(start, end, selectedText);
            return selection;
        }

        public static async System.Threading.Tasks.Task<string> GetActiveDocumentFilePathAsync(Microsoft.VisualStudio.Shell.IAsyncServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetServiceAsync(typeof(DTE));
            EnvDTE80.DTE2 applicationObject = await Task.Run(() => service)as EnvDTE80.DTE2;

            await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return applicationObject.ActiveDocument.FullName;
        }

    }
}
