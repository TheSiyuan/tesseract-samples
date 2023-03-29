using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Tesseract;
using OpenCvSharp;

namespace Tesseract.ConsoleDemo
{
    internal partial class Program
    {     
        private static Mat OpenCVprocessing(string filepath)
        {
            Mat img = Cv2.ImRead(filepath);

            // Binarize image. Text is white, background is black
            Mat bin = new Mat();
            Cv2.CvtColor(img, bin, ColorConversionCodes.BGR2GRAY);
            //Cv2.Threshold(bin, bin, 100, 255, ThresholdTypes.BinaryInv);
            Cv2.AdaptiveThreshold(bin, bin, 255, adaptiveMethod: AdaptiveThresholdTypes.GaussianC, thresholdType: ThresholdTypes.Binary, blockSize: 11, c: 2);
            // Find all white pixels
            Mat pointsMat = new Mat();
            Cv2.FindNonZero(bin, pointsMat);
            OpenCvSharp.Point[] pts = new OpenCvSharp.Point[pointsMat.Rows];
            for (int i = 0; i < pointsMat.Rows; i++)
            {
                pts[i] = pointsMat.At<Point>(i);
            }

            // Get rotated rect of white pixels
            RotatedRect box = Cv2.MinAreaRect(pts);
            if (box.Size.Width > box.Size.Height)
            {
                float temp = box.Size.Width;
                box.Size.Width = box.Size.Height;
                box.Size.Height = temp;
                box.Angle += 90.0f;
            }

            Point2f[] vertices = box.Points();

            for (int i = 0; i < 4; ++i)
            {
                Cv2.Line(img, vertices[i].ToPoint(), vertices[(i + 1) % 4].ToPoint(), Scalar.Green);
            }

            // Rotate the image according to the found angle
            Mat rotated = new Mat();
            Mat M = Cv2.GetRotationMatrix2D(box.Center, box.Angle, 1.0);
            Cv2.WarpAffine(bin, rotated, M, bin.Size());

            // Compute horizontal projections
            Mat horProj = new Mat();
            Cv2.Reduce(rotated, horProj, ReduceDimension.Column, ReduceTypes.Avg, MatType.CV_32F);

            // Remove noise in histogram. White bins identify space lines, black bins identify text lines

            double th = 0;
            Mat hist = new Mat();
            Cv2.Threshold(horProj, hist, th, 255, ThresholdTypes.BinaryInv);

            // Get mean coordinate of white white pixels groups
            List<int> ycoords = new List<int>();
            int y = 0;
            int count = 0;
            bool isSpace = false;
            for (int i = 0; i < rotated.Rows; ++i)
            {
                if (!isSpace)
                {
                    if (hist.At<byte>(i) != 0)
                    {
                        isSpace = true;
                        count = 1;
                        y = i;
                    }
                }
                else
                {
                    if (hist.At<byte>(i) == 0)
                    {
                        isSpace = false;
                        ycoords.Add(y / count);
                    }
                    else
                    {
                        y += i;
                        count++;
                    }
                }
            }

            // Draw line as final result
            Mat result = new Mat();
            Cv2.CvtColor(rotated, result, ColorConversionCodes.GRAY2BGR);
            foreach (int coord in ycoords)
            {
                Cv2.Line(result, new Point(0, coord), new Point(result.Cols, coord), Scalar.Green);
            }
            Cv2.ImWrite("C:\\Users\\swei\\Downloads\\result.tif", result);

            return result;
        }
        private static Mat OpenCVprocessing_norotation(string filepath)
        {
            Mat img = Cv2.ImRead(filepath);

            // Binarize image. Text is white, background is black
            Mat bin = new Mat();
            //Mat _bin2 = new Mat();
            Cv2.CvtColor(img, bin, ColorConversionCodes.BGR2GRAY);
            Cv2.ImWrite("C:\\Users\\swei\\Downloads\\bin1.tif", bin);
            //Cv2.MedianBlur(bin, bin, 3);
            //Cv2.ImWrite("C:\\Users\\swei\\Downloads\\bin2.tif", bin);
            Cv2.AdaptiveThreshold(bin, bin, 255, adaptiveMethod: AdaptiveThresholdTypes.GaussianC, thresholdType: ThresholdTypes.Binary, blockSize: 55, c: 11);
            Cv2.ImWrite("C:\\Users\\swei\\Downloads\\bin3.tif", bin);
            //Cv2.MedianBlur(bin, bin, 3);
            //Cv2.ImWrite("C:\\Users\\swei\\Downloads\\bin4.tif", bin);
            //Cv2.AdaptiveThreshold(bin, bin, 255, adaptiveMethod: AdaptiveThresholdTypes.GaussianC, thresholdType: ThresholdTypes.Binary, blockSize: 5, c: 2);
            //Cv2.ImWrite("C:\\Users\\swei\\Downloads\\bin3.tif", bin);
            // Find all white pixels

            Mat result = new Mat();
            Cv2.CvtColor(bin, result, ColorConversionCodes.GRAY2BGR);

            Cv2.ImWrite("C:\\Users\\swei\\Downloads\\result.tif", result);

            return result;
        }
        private static Mat OpenCVprocessing_AdaptiveThreshhold(string filepath)
        {
            Mat img = Cv2.ImRead(filepath);
            Mat bin = new Mat();
            Cv2.CvtColor(img, bin, ColorConversionCodes.BGR2GRAY);
            Cv2.AdaptiveThreshold(bin, bin, 255, adaptiveMethod: AdaptiveThresholdTypes.GaussianC, thresholdType: ThresholdTypes.Binary, blockSize: 55, c: 11);
            
            return bin;
        }
        private static Mat OpenCVprocessing_noprocessing(string filepath)
        {
            Mat img = Cv2.ImRead(filepath);
            
            return img;
        }
    }
}