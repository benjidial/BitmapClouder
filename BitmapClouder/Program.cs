/* Bitmap Clouder source */

/* Copyright 2016 Benji Dial

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.*/

using System;
using System.Drawing;
using System.IO;

namespace Benji.BitmapClouder
{
    class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static Bitmap Blur(Bitmap b)
        {
            bool d = true;
            Bitmap n = null;
            try
            {
                n = new Bitmap(b.Width, b.Height, b.PixelFormat);
                for (int y = 0; y < b.Height; y++)
                    for (int x = 0; x < b.Width; x++)
                        n.SetPixel(x, y, Color.FromArgb(
                            (b.GetPixel((x + 1 + b.Width) % b.Width, y).A + b.GetPixel((x - 1 + b.Width) % b.Width, y).A +
                            b.GetPixel(x, (y + 1 + b.Height) % b.Height).A + b.GetPixel(x, (y - 1 + b.Height) % b.Height).A) / 4,
                            (b.GetPixel((x + 1 + b.Width) % b.Width, y).R + b.GetPixel((x - 1 + b.Width) % b.Width, y).R +
                            b.GetPixel(x, (y + 1 + b.Height) % b.Height).R + b.GetPixel(x, (y - 1 + b.Height) % b.Height).R) / 4,
                            (b.GetPixel((x + 1 + b.Width) % b.Width, y).G + b.GetPixel((x - 1 + b.Width) % b.Width, y).G +
                            b.GetPixel(x, (y + 1 + b.Height) % b.Height).G + b.GetPixel(x, (y - 1 + b.Height) % b.Height).G) / 4,
                            (b.GetPixel((x + 1 + b.Width) % b.Width, y).B + b.GetPixel((x - 1 + b.Width) % b.Width, y).B +
                            b.GetPixel(x, (y + 1 + b.Height) % b.Height).B + b.GetPixel(x, (y - 1 + b.Height) % b.Height).B) / 4));
                d = false;
                return n;
            }
            finally
            {
                if (d && n != null)
                    n.Dispose();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        static int Main(string[ ] args)
        {
            string cmd = Environment.CommandLine.Trim();
            cmd = cmd[0] == '\"' ? '\"' + cmd.Split('\"')[1] + '\"' : cmd.Split(' ')[0];
            res = new System.Resources.ResourceManager("Benji.BitmapClouder.Strings", typeof(Program).Assembly);
            var cul = System.Globalization.CultureInfo.CurrentCulture;
            if (args.Length == 0)
            {
                Console.Error.WriteLine(res.GetString("NoArgs"));
                Console.Error.WriteLine(res.GetString("InvArgs"), cmd, res.GetString("HelpCmd"));
                Console.Write(res.GetString("AnyKey"));
                try
                {
                    Console.ReadKey(true);
                    Console.WriteLine();
                }
                catch (InvalidOperationException)
                {
                    Console.CursorLeft = 0;
                    Console.Write(new String(' ', res.GetString("AnyKey").Length));
                    Console.CursorLeft = 0;
                }
                return 1;
            }
            if (args[0] == res.GetString("HelpCmd"))
            {
                Console.WriteLine(res.GetString("Help"), cmd, res.GetString("HelpCmd"), res.GetString("CmdIn"),
                    res.GetString("TimesFlag"), res.GetString("Times"), res.GetString("CmdOut"), res.GetString("Open"),
                    String.Format(cul, res.GetString("DefNew"), res.GetString("CmdIn")));
                return 0;
            }
            times = 1;
            if (args.Length > 1 && args[1].StartsWith(res.GetString("TimesFlag"), StringComparison.CurrentCulture))
            {
                try
                {
                    times = int.Parse(args[1].Substring(res.GetString("TimesFlag").Length), cul);
                }
                catch (FormatException)
                {
                    Console.Error.WriteLine(res.GetString("TimesParse"), args[1].Substring(res.GetString("TimesFlag").Length));
                    return 2;
                }
                if (times < 1)
                {
                    Console.Error.WriteLine(res.GetString("TimesNotPos"), times);
                    return 3;
                }
                {
                    var argsList = new System.Collections.Generic.List<string>(args);
                    argsList.RemoveAt(1);
                    args = argsList.ToArray();
                }
            }
            if (args.Length == 1)
                args = new string[ ] { args[0], String.Format(cul, res.GetString("DefNew"), args[0]) };
            else if (args[1] == res.GetString("Open"))
                args = new string[ ] { args[0], String.Format(cul, res.GetString("DefNew"), args[0]), args[1] };
            Bitmap b, n, m;
            try
            {
                b = new Bitmap(args[0]);
            }
            catch (ArgumentException)
            {
                try
                {
                    if (File.Exists(args[0]))
                    {
                        if (File.ReadAllBytes(args[0]).Length == 0)
                        {
                            Console.Error.WriteLine(res.GetString("Blank"), args[0]);
                            return 6;
                        }
                        Console.Error.WriteLine(res.GetString("InvBit"), args[0]);
                        return 7;
                    }
                    Console.Error.WriteLine(res.GetString("FNF"), args[0]);
                    return 4;
                }
                catch (IOException ex)
                {
                    Console.Error.WriteLine(res.GetString("ErrorLoad"), args[0], ex.Message);
                    return 5;
                }
            }
            System.Timers.Timer tm = new System.Timers.Timer(2500);
            tm.Elapsed += Progress;
            Console.WriteLine(res.GetString("Start"), args[0], times);
            tm.Start();
            n = Blur(b);
            b.Dispose();
            for (t = 1; t < times; t++)
            {
                m = (Bitmap)n.Clone();
                n.Dispose();
                n = Blur(m);
                m.Dispose();
            }
            tm.Stop();
            Console.CursorLeft = 0;
            Console.WriteLine(res.GetString("Complete"));
            tm.Dispose();
            try
            {
                n.Save(args[1]);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(res.GetString("ErrorSave"), args[1], ex.Message);
                n.Dispose();
                return 8;
            }
            n.Dispose();
            if (args.Length < 3 || args[2] != res.GetString("Open"))
                return 0;
            while (true)
            {
                try
                {
                    System.Diagnostics.Process.Start(args[1]);
                    return 0;
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    Console.Error.WriteLine(res.GetString("StartWin32"), args[1], ex.Message);
                }
                catch (FileNotFoundException ex)
                {
                    Console.Error.WriteLine(res.GetString("StartFNF"), args[1], ex.Message);
                }
                while (true)
                {
                    Console.Write(String.Format(cul, res.GetString("TryAgain"),
                        res.GetString("Y")[0], res.GetString("N")[0]));
                    try
                    {
                        char ans = Console.ReadKey(true).KeyChar;
                        Console.WriteLine();
                        if (res.GetString("Y").Contains(new string(ans, 1)))
                            break;
                        else if (res.GetString("N").Contains(new string(ans, 1)))
                            return 9;
                    }
                    catch (InvalidOperationException)
                    {
                        Console.CursorLeft = 0;
                        Console.Write(new String(' ', String.Format(cul, res.GetString("TryAgain"),
                            res.GetString("Y")[0], res.GetString("N")[0]).Length));
                        Console.CursorLeft = 0;
                        return 9;
                    }
                }
            }
        }

        private static int times, t;

        private static System.Resources.ResourceManager res;

        private static void Progress(object source, System.Timers.ElapsedEventArgs e)
        {
            Console.CursorLeft = 0;
            Console.Write(res.GetString("Progress"), t, times);
        }
    }
}
