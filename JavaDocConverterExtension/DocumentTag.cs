using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaDocConverterExtension
{
    public class DocumentTag
    {
        #region Tag List
        public static DocumentTag CommentBase = new DocumentTag("/**", "/// <summary>", "", "", 0, false, false, false);
        public static DocumentTag Param = new DocumentTag("@param", @"<param name=""", "</param>", @""">", 1, true, false, true);
        public static DocumentTag TypeParam = new DocumentTag("@param", @"<typeparam name=""", "</typeparam>", @""">", 1, true, false, true);
        public static DocumentTag Return = new DocumentTag("@return", @"<returns>", "</returns>", "", 2, false, false, true);
        public static DocumentTag Throws = new DocumentTag("@throws", @"<exception cref=""", " </exception>", @""">", 3, true, false, true);
        public static DocumentTag Exception = new DocumentTag("@exception", @"<exception cref=""", " </exception>", @""">", 4, true, false, true);
        public static DocumentTag See = new DocumentTag("@see", @"<see cref=""", " </see>", @""">", 5, true, false, true);
        public static DocumentTag CommentBaseEnd = new DocumentTag("*/", "/// <summary>", "", "", 6, false, false, false);
        public static DocumentTag CommentBody = new DocumentTag("*", "/// ", "", "", 7, false, false, false);
        #endregion

        private String _javaDocTag;
        private String _csharpDocTagBegin;
        private String _csharpDocTagEnd;
        private String _csharpDocTagBeginClose;
        private int _ordinal;
        private Boolean _isNamedElement;
        private Boolean _deleteLine;
        private Boolean _needCloseTag;

        public String JavaDocTag
        {
            get { return _javaDocTag; }
        }

        public String CSharpDocTagBegin
        {
            get { return _csharpDocTagBegin; }
        }

        public String CSharpDocTagEnd
        {
            get { return _csharpDocTagEnd; }
        }

        public String CSharpDocTagBeginClose
        {
            get { return _csharpDocTagBeginClose; }
        }

        public int Ordinal
        {
            get { return _ordinal; }
        }

        public Boolean IsNamedElement
        {
            get { return _isNamedElement; }
        }

        public Boolean DeleteLine
        {
            get { return _deleteLine; }
        }

        public Boolean NeedCloseTag
        {
            get { return _needCloseTag; }
        }

        public static DocumentTag Get(int ordinal)
        {
            switch (ordinal)
            {
                case 0:
                    return CommentBase;
                case 1:
                    return Param;
                case 2:
                    return Return;
                case 3:
                    return Throws;
                case 4:
                    return Exception;
                case 5:
                    return See;
                case 6:
                    return CommentBaseEnd;
                case 7:
                    return CommentBody;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        private DocumentTag(String javaDocTag, String csharpDocTagBegin, String csharpDocTagEnd, String csharpDocTagBeginClose, int ordinal, Boolean isNamedElement, Boolean deleteLine, Boolean needCloseTag)
        {
            _javaDocTag = javaDocTag;
            _csharpDocTagBegin = csharpDocTagBegin;
            _csharpDocTagEnd = csharpDocTagEnd;
            _csharpDocTagBeginClose = csharpDocTagBeginClose;
            _ordinal = ordinal;
            _isNamedElement = isNamedElement;
            _deleteLine = deleteLine;
            _needCloseTag = needCloseTag;
        }
    }
}
