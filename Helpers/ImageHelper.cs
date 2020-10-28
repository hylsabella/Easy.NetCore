using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Easy.Common.NetCore.Helpers
{
    public static class ImageHelper
    {
        public static Image BytesToImage(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            Image image = Image.FromStream(ms);
            return image;
        }

        public static string CreateImageFromBytes(string fileName, byte[] buffer)
        {
            string file = fileName;

            Image image = BytesToImage(buffer);

            ImageFormat format = image.RawFormat;

            if (format.Equals(ImageFormat.Jpeg))
            {
                file += ".jpeg";
            }
            else if (format.Equals(ImageFormat.Png))
            {
                file += ".png";
            }
            else if (format.Equals(ImageFormat.Bmp))
            {
                file += ".bmp";
            }
            else if (format.Equals(ImageFormat.Gif))
            {
                file += ".gif";
            }
            else if (format.Equals(ImageFormat.Icon))
            {
                file += ".icon";
            }

            FileInfo info = new FileInfo(file);

            Directory.CreateDirectory(info.Directory.FullName);

            File.WriteAllBytes(file, buffer);

            return file;
        }
    }
}
