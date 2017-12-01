using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Helpers;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace BobDono.Utils
{
    public static class BracketImageGenerator
    {
        private const int Spacing = 10;
        private const int FontSize = 42;
        private const int UpperBarHeight = 42;
        private const int LeftRightMargins = 0;
        private const int TopBottomMargin = 0;

        private static Font _font;

        static BracketImageGenerator()
        {
            var coll = new FontCollection();
            coll.Install("Fonts/Roboto-Regular.ttf");
            _font = coll.CreateFont("Roboto", FontSize);
        }

        public static Stream Generate(List<byte[]> rawImages)
        {
            var images = new List<Image<Rgba32>>();
            foreach (var rawImage in rawImages)
                images.Add(Image.Load(rawImage));


            var currentX = LeftRightMargins;
            int i = 1;

            int avgHeight = 0;
            if (rawImages.Count == 3) //take average of two smaller ones
            {
                var maxHeight = images.Max(image => image.Height);
                var imgs = images.ToList();
                imgs.Remove(imgs.First(image => image.Height == maxHeight));
                avgHeight = (int) imgs.Average(image => image.Height);
            }
            else //just average
            {
                avgHeight = (int) images.Average(image => image.Height);
            }

            var img = new Image<Rgba32>(
                images.Sum(image => (int) (image.Width * ((float) avgHeight / image.Height))) + Spacing *
                (images.Count -
                 1) + LeftRightMargins * 2, avgHeight + UpperBarHeight + TopBottomMargin * 2);


            //img.Mutate(ctx => ctx.BackgroundColor(new Rgba32(47, 50, 55)));
            //img.Mutate(ctx => ctx.BackgroundColor(new Rgba32(50, 54, 59),
            //    new Rectangle(new Point(1, 1), new Size(img.Width - 2, img.Height - 2))));
            //img.Mutate(ctx => ctx.DrawLines(new Pen<Rgba32>(new Rgba32(165, 42, 42), 4),
            //    new[] {new PointF(2f,0), new PointF( 2f, img.Height) }));


            try
            {
                foreach (var image in images)
                {
                    var scale = (float) avgHeight / image.Height;
                    image.Mutate(context =>
                        context.Resize(new Size((int) (image.Width * scale), (int) (image.Height * scale))));

                    img.Mutate(context =>
                        context.DrawImage(image, PixelBlenderMode.Normal, 1, image.Size(),
                            new Point(currentX, UpperBarHeight + TopBottomMargin)));
                    img.Mutate(context =>
                        context.DrawText($"{i++}", _font, Rgba32.White,
                            new Point(currentX + image.Width / 2 - FontSize / 2,
                                (UpperBarHeight + TopBottomMargin - FontSize) / 2 - (int)(FontSize / 3.5))));
                    currentX += image.Width + Spacing;
                }
            }
            catch (Exception e)
            {

            }
            var ms = new MemoryStream();
            img.SaveAsPng(ms);
            return ms;
        }
    }
} 
