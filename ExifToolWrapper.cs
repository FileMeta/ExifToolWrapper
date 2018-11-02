/*
---
name: ExifToolWrapper.cs
description: C# Wrapper for Phil Harvey's excellent ExifTool
url: https://github.com/FileMeta/ExifToolWrapper/raw/master/ExifToolWrapper.cs
version: 1.0
keywords: CodeBit
dateModified: 2018-11-02
license: http://unlicense.org
about: https://sno.phy.queensu.ca/~phil/exiftool/
# Metadata in MicroYaml format. See http://filemeta.org/CodeBit.html
...
*/

/*
Unlicense: http://unlicense.org

This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or distribute
this software, either in source code form or as a compiled binary, for any
purpose, commercial or non-commercial, and by any means.

In jurisdictions that recognize copyright laws, the author or authors of this
software dedicate any and all copyright interest in the software to the
public domain. We make this dedication for the benefit of the public at large
and to the detriment of our heirs and successors. We intend this dedication
to be an overt act of relinquishment in perpetuity of all present and future
rights to this software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org/>
*/
//#define EXIF_TRACE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace ExifToolWrapper
{
    class ExifTool : IDisposable
    {
        const string c_exeName = "exiftool.exe";
        const string c_arguments = @"-stay_open 1 -@ - -common_args -charset UTF8 -G1 -args";
        const string c_exitCommand = "-stay_open\n0\n-execute\n";
        const int c_timeout = 30000;    // in milliseconds
        const int c_exitTimeout = 15000;

        static Encoding s_Utf8NoBOM = new UTF8Encoding(false);

        Process m_exifTool;
        StreamWriter m_in;
        StreamReader m_out;

        public ExifTool()
        {
            // Prepare process start
            var psi = new ProcessStartInfo(c_exeName, c_arguments);
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.StandardOutputEncoding = s_Utf8NoBOM;

            m_exifTool = Process.Start(psi);
            if (m_exifTool == null || m_exifTool.HasExited)
            {
                throw new ApplicationException("Failed to launch ExifTool!");
            }

            // ProcessStartInfo in .NET Framework doesn't have a StandardInputEncoding property (though it does in .NET Core)
            // So, we have to wrap it this way.
            m_in = new StreamWriter(m_exifTool.StandardInput.BaseStream, s_Utf8NoBOM);
            m_out = m_exifTool.StandardOutput;
        }

        public void GetProperties(string filename, ICollection<KeyValuePair<string, string> > propsRead)
        {
            m_in.Write(filename);
            m_in.Write("\n-execute\n");
            m_in.Flush();
#if EXIF_TRACE
            Debug.WriteLine(filename);
            Debug.WriteLine("-execute");
#endif
            for (; ; )
            {
                var line = m_out.ReadLine();
#if EXIF_TRACE
                Debug.WriteLine(line);
#endif
                if (line.StartsWith("{ready")) break;
                if (line[0] == '-')
                {
                    int eq = line.IndexOf('=');
                    if (eq > 1)
                    {
                        string key = line.Substring(1, eq - 1);
                        string value = line.Substring(eq + 1).Trim();
                        propsRead.Add(new KeyValuePair<string, string>(key, value));                      
                    }
                }
            }
        }

    #region IDisposable Support

    protected virtual void Dispose(bool disposing)
        {
            if (m_exifTool != null)
            {
                if (!disposing)
                {
                    System.Diagnostics.Debug.Fail("Failed to dispose ExifTool.");
                }

                // If process is running, shut it down cleanly
                if (!m_exifTool.HasExited)
                {
                    m_in.Write(c_exitCommand);
                    m_in.Flush();

                    if (!m_exifTool.WaitForExit(c_exitTimeout))
                    {
                        m_exifTool.Kill();
                        Debug.Fail("Timed out waiting for exiftool to exit.");
                    }
#if EXIF_TRACE
                    else
                    {
                        Debug.WriteLine("ExifTool exited cleanly.");
                    }
#endif
                }

                if (m_out != null)
                {
                    m_out.Dispose();
                    m_out = null;
                }
                if (m_in != null)
                {
                    m_in.Dispose();
                    m_in = null;
                }
                m_exifTool.Dispose();
                m_exifTool = null;
            }
        }

        ~ExifTool()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
