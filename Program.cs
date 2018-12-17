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

                TestDateParser();
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

        static readonly string[] s_dateFailureTests = new string[]
        {
            "2018-12-17 15:26:23", // Dashes instead of colons on the date section
            "2018:11:31 15:26:23", // November only has 30 days
            "2017:02:29 15:26:23", // February only has 28 days
            "2017:02:29 24:26:23", // No 24th hour
        };

    static readonly int[] s_dayLimit = new int[] {0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        static bool TestDateParser()
        {
            // Try a full decade
            for (int year=1995; year <= 2004; ++year)
            {
                for (int month=1; month <= 12; ++month)
                {
                    int dayLimit = s_dayLimit[month];
                    if (dayLimit == 28 && year % 4 == 0) dayLimit = 29;
                    for (int day=1; day <= dayLimit; ++day)
                    {
                        if (!TestDateParser(year, month, day, month, day, month + day)) return false;
                    }
                }
            }

            // Try a full day
            for (int hour=0; hour<24; ++hour)
            {
                for (int minute=0; minute<60; ++minute)
                {
                    for (int second=0; second<60; ++second)
                    {
                        if (!TestDateParser(2000 + hour + minute, hour % 12 + 1, minute % 28 + 1, hour, minute, second)) return false;
                    }
                }
            }

            foreach(var s in s_dateFailureTests)
            {
                if (!TestDateParserError(s)) return false;
            }

            Console.WriteLine("Date parsing tests passed.");
            return true;
        }

        static bool TestDateParser(int year, int month, int day, int hour, int minute, int second)
        {
            string dtFormatted = string.Format("{0:D4}:{1:D2}:{2:D2} {3:D2}:{4:D2}:{5:D2}", year, month, day, hour, minute, second);
            DateTime dtParsed;
            if (!ExifTool.TryParseDate(dtFormatted, DateTimeKind.Unspecified, out dtParsed))
            {
                Console.WriteLine("Date Parse failed to parse: {0}", dtFormatted);
                return false;
            }

            DateTime dtComposed = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified);
            if (!dtParsed.Equals(dtComposed))
            {
                Console.WriteLine("Date Parse doesn't match: {0}", dtFormatted);
                return false;
            }
            return true;
        }

        static bool TestDateParserError(string failString)
        {
            DateTime dtParsed;
            if (ExifTool.TryParseDate(failString, DateTimeKind.Unspecified, out dtParsed))
            {
                Console.WriteLine("Date parse should have failed: {0}", failString);
                return false;
            }
            return true;
        }
    }
}
