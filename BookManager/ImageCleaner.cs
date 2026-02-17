using System;
using System.Collections.Generic;
using System.Text;

namespace BookManager
{
    public static class ImageCleaner
    {
        public static void CleanupTempImage(string imagePath)
        {
            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            {
                try
                {
                    File.Delete(imagePath);
                }
                catch
                {

                }
            }
        }
    }
}
