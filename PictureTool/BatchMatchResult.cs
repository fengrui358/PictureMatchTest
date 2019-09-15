using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PictureTool
{
    public class BatchMatchResult
    {
        private static readonly Random Random = new Random();
        private readonly string _source;

        public Rectangle? SubRectangle;

        public string FileName => Path.GetFileName(_source);

        public BatchMatchResult(string path)
        {
            _source = path;
        }

        private string _algorithmDescription = string.Empty;

        public string AlgorithmDescription
        {
            get => _algorithmDescription.Trim();
            set { _algorithmDescription = value; }
        }

        public Bitmap GetBitmap()
        {
            return new Bitmap(_source);
        }

        public Bitmap GetSubBitmap()
        {
            using (var source = new Bitmap(_source))
            {
                if (SubRectangle == null)
                {
                    var x = Random.Next(0, source.Size.Width);
                    var y = Random.Next(0, source.Size.Height);

                    var subWidth = Random.Next(x, source.Size.Width + 1) - x;
                    var subHeight = Random.Next(y, source.Size.Height + 1) - y;

                    SubRectangle = new Rectangle(x, y, subWidth, subHeight);
                }

                using (var bmpOut = new Bitmap(SubRectangle.Value.Width, SubRectangle.Value.Height,
                    PixelFormat.Format32bppArgb))
                {
                    var g = Graphics.FromImage(bmpOut);
                    g.DrawImage(source, new Rectangle(0, 0, bmpOut.Size.Width, bmpOut.Size.Height), SubRectangle.Value,
                        GraphicsUnit.Pixel);
                    g.Dispose();

                    using (var stream = new MemoryStream())
                    {
                        bmpOut.Save(stream, ImageFormat.Bmp);

                        return new Bitmap(stream);
                    }
                }
            }
        }
    }
}