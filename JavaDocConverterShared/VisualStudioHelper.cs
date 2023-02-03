using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace JavaDocConverterExtension
{
    using IBindCtx = System.Runtime.InteropServices.ComTypes.IBindCtx;
    using Process = System.Diagnostics.Process;
    using IRunningObjectTable = System.Runtime.InteropServices.ComTypes.IRunningObjectTable;
    using IEnumMoniker = System.Runtime.InteropServices.ComTypes.IEnumMoniker;
    using IMoniker = System.Runtime.InteropServices.ComTypes.IMoniker;
    using Thread = System.Threading.Thread;

    public static class VisualStudioHelper
    {

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        /// <summary>
        /// Gets the DTE object from any devenv process.
        /// </summary>
        /// <remarks>
        /// After starting devenv.exe, the DTE object is not ready. We need to try repeatedly and fail after the
        /// timeout.
        /// </remarks>
        /// <param name="processId">
        /// <param name="timeout">Timeout in seconds.
        /// <returns>
        /// Retrieved DTE object or <see langword="null"> if not found.
        /// </see></returns>
        public static EnvDTE80.DTE2 GetDTE(int processId, int timeout)
        {
            EnvDTE80.DTE2 res = null;
            DateTime startTime = DateTime.Now;

            while (res == null && DateTime.Now.Subtract(startTime).Seconds < timeout)
            {
                Thread.Sleep(1000);
                res = GetDTE(processId);
            }

            return res;
        }

        /// <summary>
        /// Gets the DTE object from any devenv process.
        /// </summary>
        /// <param name="processId">
        /// <returns>
        /// Retrieved DTE object or <see langword="null"> if not found.
        /// </see></returns>
        public static EnvDTE80.DTE2 GetDTE(int processId)
        {
            object runningObject = null;

            IBindCtx bindCtx = null;
            IRunningObjectTable rot = null;
            IEnumMoniker enumMonikers = null;

            try
            {
                Marshal.ThrowExceptionForHR(CreateBindCtx(reserved: 0, ppbc: out bindCtx));
                bindCtx.GetRunningObjectTable(out rot);
                rot.EnumRunning(out enumMonikers);

                IMoniker[] moniker = new IMoniker[1];
                IntPtr numberFetched = IntPtr.Zero;
                while (enumMonikers.Next(1, moniker, numberFetched) == 0)
                {
                    IMoniker runningObjectMoniker = moniker[0];

                    string name = null;

                    try
                    {
                        if (runningObjectMoniker != null)
                        {
                            runningObjectMoniker.GetDisplayName(bindCtx, null, out name);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Do nothing, there is something in the ROT that we do not have access to.
                    }

                    Regex monikerRegex = new Regex(@"!VisualStudio.DTE\.\d+\.\d+\:" + processId, RegexOptions.IgnoreCase);
                    if (!string.IsNullOrEmpty(name) && monikerRegex.IsMatch(name))
                    {
                        Marshal.ThrowExceptionForHR(rot.GetObject(runningObjectMoniker, out runningObject));
                        break;
                    }
                }
            }
            finally
            {
                if (enumMonikers != null)
                {
                    Marshal.ReleaseComObject(enumMonikers);
                }

                if (rot != null)
                {
                    Marshal.ReleaseComObject(rot);
                }

                if (bindCtx != null)
                {
                    Marshal.ReleaseComObject(bindCtx);
                }
            }

            return runningObject as EnvDTE80.DTE2;
        }
    }
}
