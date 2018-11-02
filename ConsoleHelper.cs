/*
---
# Metadata in MicroYaml format. See http://www.filemeta.org/CodeBit.html
name: ConsoleHelper.cs
description: Helper class for console applications in C#
url: https://github.com/FileMeta/ConsoleHelper/raw/master/ConsoleHelper.cs
version: 1.1
keywords: codebit
dateModified: 2018-04-30
license: http://unlicense.org
...
*/
/*
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org/>
*/

using System;
using System.Runtime.InteropServices;

namespace Win32Interop
{

    /// <summary>
    /// Set of static properties and methods that make console applicaitons operate more smoothly.
    /// </summary>
    static class ConsoleHelper
    {

        #region Private Stuff

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetConsoleProcessList(uint[] ProcessList, uint ProcessCount);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion Private Stuff

        #region Public Stuff

        /// <summary>
        /// Returns true if application is the sole owner of the current console.
        /// </summary>
        public static bool IsSoleConsoleOwner
        {
            get
            {
                uint[] procIds = new uint[4];
                uint count = GetConsoleProcessList(procIds, (uint)procIds.Length);
                return count <= 1;
            }
        }

        /// <summary>
        /// If applicaiton is the sole console owner, prompts the user to press
        /// any key before returning - presumably for the application to exit.
        /// </summary>
        public static void PromptAndWaitIfSoleConsole()
        {
            if (IsSoleConsoleOwner)
            {
                var oldColor = Console.ForegroundColor;
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Press any key to exit.");
                Console.ForegroundColor = oldColor;
                Console.ReadKey(true);
            }
        }

        /// <summary>
        /// Brings the console to the front
        /// </summary>
        public static void BringConsoleToFront()
        {
            SetForegroundWindow(GetConsoleWindow());
        }

        #endregion Public Stuff
    }
}
