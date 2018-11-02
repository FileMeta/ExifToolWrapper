using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExifToolWrapper
{
    /// <summary>
    /// Test program for ExifToolWrapper
    /// </summary>
    class Program
    {
        const string c_helpText =
@"Syntax: ExifToolWrapper <filepath> ...

This is a test program for the ExifToolWrapper CodeBit. For each filename
specified on the command line it dumps all metadata retrieved by ExifTool.
The filename may contain wildcards. Any file type supported by ExifTool
may be used. If the file is not supported by ExifTool it should gracefully
report an error.";

        static ExifTool s_exifTool;

        static void Main(string[] args)
        { 
            try
            {
                if (args.Length == 0)
                {
                    args = new string[] { "-h" };
                }

                s_exifTool = new ExifTool();

                foreach (var arg in args)
                {
                    if (arg.Equals("-h", StringComparison.OrdinalIgnoreCase)
                        || arg.Equals("-?", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine(c_helpText);
                    }

                    ProcessFiles(arg);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }
            finally
            {
                if (s_exifTool != null)
                {
                    // this is the paranoid way to dispose. It handles the case were ExifTool.Dispose() throws an exception.
                    var temp = s_exifTool;
                    s_exifTool = null;
                    temp.Dispose();
                }
            }

            Win32Interop.ConsoleHelper.PromptAndWaitIfSoleConsole();
        }

        static void ProcessFiles(string path)
        {
            string folder = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(folder)) folder = Environment.CurrentDirectory;
            string pattern = Path.GetFileName(path);

            int count = 0;
            foreach(string filepath in Directory.GetFiles(folder, pattern))
            {
                ProcessFile(filepath);
                ++count;
            }
            if (count == 0)
            {
                Console.WriteLine($"No matches found for '{path}'");
            }
        }

        static void ProcessFile(string path)
        {
            Console.WriteLine(path);
            var properties = new List<KeyValuePair<string, string>>();
            s_exifTool.GetProperties(path, properties);
            foreach(var prop in properties)
            {
                Console.WriteLine($"{prop.Key}: {prop.Value}");
            }
            Console.WriteLine();
        }


    }
}
