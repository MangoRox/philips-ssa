using OpenCvSharp;

namespace SystemSetupAutomation.ImageProcessing
{
    internal sealed class ScreenshotClassifier
    {
        private const int MinSaturation = 50;
        private const int MinValue = 60;

        public static string Classify(string screenshotPath)
        {
            Console.WriteLine("Classifying screenshot '{0}'...", screenshotPath);

            using var bgr = Cv2.ImRead(screenshotPath, ImreadModes.Color);
            if (bgr.Empty())
            {
                Console.WriteLine("ERROR: could not load screenshot '{0}' into OpenCV.", screenshotPath);
                return "unknown";
            }

            using var hsv = new Mat();
            Cv2.CvtColor(bgr, hsv, ColorConversionCodes.BGR2HSV);

            var (redPixels, greenPixels, yellowPixels) = CountColorPixels(hsv);

            Console.WriteLine("red={0} yellow={1} green={2}", redPixels, yellowPixels, greenPixels);

            int best = Math.Max(redPixels, Math.Max(yellowPixels, greenPixels));

            return best switch
            {
                0 => "unknown",
                _ when best == greenPixels => "success",
                _ when best == yellowPixels => "warning",
                _ => "error"
            };
        }

        private static (int Red, int Green, int Yellow) CountColorPixels(Mat hsv)
        {
            int redPixels = 0;
            int greenPixels = 0;
            int yellowPixels = 0;

            var indexer = hsv.GetGenericIndexer<Vec3b>();
            int columnLimit = hsv.Cols / 4;

            for (int row = 0; row < hsv.Rows; row++)
            {
                for (int col = 0; col < columnLimit; col++)
                {
                    var pixel = indexer[row, col];
                    int h = pixel.Item0;
                    int s = pixel.Item1;
                    int v = pixel.Item2;

                    if (s < MinSaturation || v < MinValue)
                    {
                        continue;
                    }

                    if (h <= 10)
                    {
                        redPixels++;
                    }
                    else if (h >= 20 && h <= 30)
                    {
                        yellowPixels++;
                    }
                    else if (h >= 55 && h <= 60)
                    {
                        greenPixels++;
                    }
                }
            }

            return (redPixels, greenPixels, yellowPixels);
        }
    }
}
