using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputSimulator.Helpers
{
    public static class BitmapHelpers
    {
        public static void CompareImages(string path1, string path2)
        {
            FileInfo originalFile = new FileInfo(path1);
            FileInfo referenceFile = new FileInfo(path2);

            if (originalFile.Exists && referenceFile.Exists)
            {
                Bitmap originalBmp = new Bitmap(path1);
                Bitmap referenceBmp = new Bitmap(path2);

                if (originalBmp.Size == referenceBmp.Size)
                {
                    double similarity = Similarity(originalBmp, referenceBmp);
                    Console.WriteLine(similarity);
                }
            }

        }

        public static double Similarity(Bitmap bmp1, Bitmap bmp2)
        {
            List<KeyValuePair<int, int>> differentPixels = new List<KeyValuePair<int, int>>();

            for (int x = 0; x < bmp1.Width; x++)
            {
                for (int y = 0; y < bmp1.Height; y++)
                {
                    if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
                    {
                        differentPixels.Add(new KeyValuePair<int, int>(x, y));
                    }
                }
            }

            double totalPixelCount = bmp1.Width * bmp1.Height;
            double similarPixelCount = totalPixelCount - differentPixels.Count;

            return similarPixelCount / totalPixelCount;
        }
    }
}
