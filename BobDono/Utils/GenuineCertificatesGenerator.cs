using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Helpers;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace BobDono.Utils
{
    public class GenuineCertificatesGenerator
    {
        private const int FontSize = 34;
        private const int SmallerFontSize = 30;

        private static Font _font;
        private static Font _smallerFont;

        static GenuineCertificatesGenerator()
        {
            var coll = new FontCollection();
            coll.Install("Fonts/rounded-mgenplus-1cp-regular.ttf");
            _font = coll.CreateFont("Rounded Mgen+ 1cp regular", FontSize * 2);
            _smallerFont = coll.CreateFont("Rounded Mgen+ 1cp regular", SmallerFontSize * 2);
        }

        public static Stream Generate(List<string> lines)
        {
            var img = Image.Load($"{AppContext.BaseDirectory}/Assets/certificate.png");

            var txt = new Image<Rgba32>(342 * 2, 93 * 2);

            if (lines.Count == 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    Font font = _font;
                    if (lines[i].Length < 18)
                        lines[i] = PadString(lines[i]);
                    else
                        font = _smallerFont;

                    txt.Mutate(c =>
                    {
                        c.DrawText(lines[i], font, Rgba32.Black, new Point(342, i * 83),
                            new TextGraphicsOptions(true) {HorizontalAlignment = HorizontalAlignment.Center});
                    });
                }
            }
            else if (lines.Count == 1)
            {
                lines[0] = PadString(lines[0]);
                txt.Mutate(c =>
                {
                    c.DrawText(lines[0], _font, Rgba32.Black, new Point(342, 93 / 2),
                        new TextGraphicsOptions(true) {HorizontalAlignment = HorizontalAlignment.Center});
                });
            }

            string PadString(string s)
            {
                var len = s.Length;
                var pad = (int) Math.Ceiling((18 - len) / 2f);
                s = s.Trim().PadLeft(len + pad, ' ');
                return s.PadRight(18, ' ');
            }

            txt.Mutate(c =>
            {
                c.Rotate(-48, true);
                c.Resize(txt.Size() / 2);
            });

            img.Mutate(context => context.DrawImage(txt, PixelBlenderMode.Normal, 1, txt.Size(), new Point(484, 34)));
            //img.Save("test.png");

            var ms = new MemoryStream();
            img.SaveAsPng(ms);
            return ms;
        }
    }
}
