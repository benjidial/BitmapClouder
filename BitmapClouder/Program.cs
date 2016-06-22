/* Bitmap Clouder source, Copyright 2016 Benji Dial under MIT license */
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Resources;

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
            ResourceManager res = new ResourceManager("Benji.BitmapClouder.Strings", typeof(Program).Assembly);
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
                    String.Format(CultureInfo.CurrentCulture, res.GetString("DefNew"), res.GetString("CmdIn")));
                return 0;
            }
            if (args.Length == 1)
                args = new string[ ] { args[0], String.Format(CultureInfo.CurrentCulture, res.GetString("DefNew"), args[0]) };
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
            return 0;
        }
    }
}
