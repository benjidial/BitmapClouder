/* Bitmap Clouder source, Copyright 2016 Benji Dial under MIT license */
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
            var res = new System.Resources.ResourceManager("Benji.BitmapClouder.Strings", typeof(Program).Assembly);
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
                Console.WriteLine(res.GetString("Help"), cmd, res.GetString("HelpCmd"), res.GetString("CmdIn"), res.GetString("CmdOut"),
                    res.GetString("Open"), String.Format(cul, res.GetString("DefNew"), res.GetString("CmdIn")));
                return 0;
            }
            if (args.Length == 1)
                args = new string[ ] { args[0], String.Format(cul, res.GetString("DefNew"), args[0]) };
            else if (args[1] == res.GetString("Open"))
                args = new string[ ] { args[0], String.Format(cul, res.GetString("DefNew"), args[0]), args[1] };
            Bitmap b, n;
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
                            return 4;
                        }
                        Console.Error.WriteLine(res.GetString("InvBit"), args[0]);
                        return 5;
                    }
                    Console.Error.WriteLine(res.GetString("FNF"), args[0]);
                    return 2;
                }
                catch (IOException ex)
                {
                    Console.Error.WriteLine(res.GetString("ErrorLoad"), args[0], ex.Message);
                    return 3;
                }
            }
            n = Blur(b);
            b.Dispose();
            try
            {
                n.Save(args[1]);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(res.GetString("ErrorSave"), args[1], ex.Message);
                n.Dispose();
                return 6;
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
                            return 7;
                    }
                    catch (InvalidOperationException)
                    {
                        Console.CursorLeft = 0;
                        Console.Write(new String(' ', String.Format(cul, res.GetString("TryAgain"),
                            res.GetString("Y")[0], res.GetString("N")[0]).Length));
                        Console.CursorLeft = 0;
                        return 7;
                    }
                }
            }
        }
    }
}
