﻿using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Microsoft.ProjectOxford.Vision;

namespace AutoAltTags
{
    class DescribeImage
    {
        private readonly string _apiKey;

        public DescribeImage(string apiKey)
        {
            _apiKey = apiKey;
        }

        public string GetDescription(Stream stream)
        {
            string description = string.Empty;
            stream = ResizeImage(stream, 2048);

            VisionServiceClient visionServiceClient = new VisionServiceClient(_apiKey);
            var analysisResult = visionServiceClient.DescribeAsync(stream);

            if (analysisResult.Result.Description.Captions != null && analysisResult.Result.Description.Captions.Any())
                description = analysisResult.Result.Description.Captions.First().Text;

            return description;
        }

        private static Stream ResizeImage(Stream imageStream, int maxSize)
        {
            using (Image oldImage = Image.FromStream(imageStream))
            {
                Size newSize = CalculateDimensions(oldImage.Size, maxSize);

                if (newSize != oldImage.Size)
                {
                    using (Bitmap newImage = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format24bppRgb))
                    {
                        using (Graphics canvas = Graphics.FromImage(newImage))
                        {
                            canvas.SmoothingMode = SmoothingMode.AntiAlias;
                            canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            canvas.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            canvas.DrawImage(oldImage, new Rectangle(new Point(0, 0), newSize));
                            MemoryStream m = new MemoryStream();
                            newImage.Save(m, ImageFormat.Jpeg);
                            return m;
                        }
                    }
                }
            }

            return imageStream;
        }

        private static Size CalculateDimensions(Size oldSize, int maxSize)
        {
            Size newSize = new Size(oldSize.Width, oldSize.Height);

            if (oldSize.Height > oldSize.Width && oldSize.Height > maxSize)
            {
                newSize.Width = (int)(oldSize.Width * ((float)maxSize / oldSize.Height));
                newSize.Height = maxSize;
            }
            else if (oldSize.Height > oldSize.Width && oldSize.Width > maxSize)
            {
                newSize.Width = maxSize;
                newSize.Height = (int)(oldSize.Height * ((float)maxSize / oldSize.Width));
            }

            return newSize;
        }
    }
}
