using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace BobDono.Utils
{
    public class GenuineCertificatesGenerator
    {
        private const int FontSize = 17;
        private const int SmallerFontSize = 15;
        private const int KanjiFontSize = 13;
        private const int CharactersPerLine = 15;

        private static Font _font;
        private static Font _smallerFont;
        private static Font _kanjiFont;

        static GenuineCertificatesGenerator()
        {
            var coll = new FontCollection();
            coll.Install("Fonts/togoshi-mono.ttf");
            _font = coll.CreateFont("Togoshi Mono", FontSize * 2);
            _smallerFont = coll.CreateFont("Togoshi Mono", SmallerFontSize * 2);
            _kanjiFont = coll.CreateFont("Togoshi Mono", KanjiFontSize * 2);

        }

        public static Stream Generate(List<string> lines)
        {
            var img = Image.Load($"{AppContext.BaseDirectory}/Assets/certificate.png");

            var txt = new Image<Rgba32>(342, 93);

            if (lines.Count == 2)
            {
                for (int i = 0; i < 2; i++)
                {

                    Font font = _font;
                    if (lines[i].Length < CharactersPerLine - CharactersPerLine / 4)
                        lines[i] = PadString(lines[i]);
                    else
                    {
                        font = HasMoonrunes(lines[i]) ? _kanjiFont : _smallerFont;
                    }

                    txt.Mutate(c =>
                    {
                        c.DrawText(
                            new TextGraphicsOptions(true) { HorizontalAlignment = HorizontalAlignment.Center },
                            lines[i],
                            font,
                            Rgba32.Black,
                            new Point(171, i * 83/2));
                    });
                }
            }
            else if (lines.Count == 1)
            {
                var font = lines[0].Length >= CharactersPerLine / 1.5
                    ? (HasMoonrunes(lines[0]) ? _kanjiFont : _font)
                    : _font;
                lines[0] = PadString(lines[0]);
                txt.Mutate(c =>
                    {
                        c.DrawText(new TextGraphicsOptions(true) {HorizontalAlignment = HorizontalAlignment.Center},
                            lines[0], font, Rgba32.Black, new Point(171, 93 / 4));
                    });
            }

            string PadString(string s)
            {
                var len = s.Length;
                var pad = (int) Math.Ceiling((CharactersPerLine - len) / 2f);
                s = s.Trim().PadLeft(len + pad, ' ');
                return s.PadRight(CharactersPerLine, ' ');
            }

            bool HasMoonrunes(string s)
            {
                return GetCharsInRange(s, 0x3040, 0x309F).Any() || //h
                       GetCharsInRange(s, 0x30A0, 0x30FF).Any() || //k
                       GetCharsInRange(s, 0x4E00, 0x9FBF).Any(); //kanji

                IEnumerable<char> GetCharsInRange(string text, int min, int max)
                {
                    return text.Where(e => e >= min && e <= max);
                }
            }

            txt.Mutate(c =>
            {
                c.Rotate(-48);
                c.Resize(txt.Size() / 2);
            });

            img.Mutate(context => context.DrawImage(txt, new Point(250, 18), PixelColorBlendingMode.Normal, 1));

            var ms = new MemoryStream();
            img.SaveAsPng(ms);
            return ms;
        }
    }
}
