﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing;
using System.IO;

namespace Tesseract.ConsoleDemo
{
    internal partial class Program
    {
        public static void Main(string[] args)
        {
            //var testImagePath = "./phototest.tif";
            var testImagePath = "C:\\Users\\swei\\Downloads\\receipt\\5.jpg";
            if (args.Length > 0)
            {
                testImagePath = args[0];
            }
            string text;
            text = ReadUsingTesseract(OpenCVProcessMethod(testImagePath, 0));
            MatchTextItems(text);
            text = ReadUsingTesseract(OpenCVProcessMethod(testImagePath, 1));
            MatchTextItems(text);
            text = ReadUsingTesseract(OpenCVProcessMethod(testImagePath, 2));
            MatchTextItems(text);

            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }
        static decimal ProcessString(string input)
        {
            // Remove all spaces
            input = input.Replace(" ", string.Empty);

            // Remove all non-number characters
            input = Regex.Replace(input, @"\D", string.Empty);

            // Check if the input has at least 3 digits
            if (input.Length < 3)
            {
                throw new ArgumentException("The input string should have at least 3 digits.");
            }

            // Add a decimal point between the last 2 and 3 digit
            int decimalPosition = input.Length - 2;
            input = input.Insert(decimalPosition, ".");

            // Convert the modified string to a decimal number
            decimal result = decimal.Parse(input);

            return result;
        }

        internal static string ReadUsingTesseract(byte[] bitmapBytes)
        {
            try
            {
                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.TesseractAndLstm))
                {
                    //using (var img = Pix.LoadFromFile(testImagePath))
                    using (Pix img = Pix.LoadFromMemory(bitmapBytes))
                    {
                        using (var page = engine.Process(img))
                        {
                            var text = page.GetText();
                            Console.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());
                            //Console.WriteLine("Text (GetText): \r\n{0}", text);
                            return text;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Console.WriteLine("Unexpected Error: " + e.Message);
                Console.WriteLine("Details: ");
                Console.WriteLine(e.ToString());
                return null;
            }
        }
        static void MatchTextItems(string text)
        {
            // Extract receipt information using regex patterns
            if (text == null)
            {
                Console.WriteLine("Failed to read anything from the photo");
                return;
            }
            var totalCostPattern1 = @"(?i)total\s*(?:cost)?.*:??\s*\$?\s*(\d+\.?\s?\d{2})\n";
            var totalCostPattern2 = @"((?i)total\s*(?:cost)?.*:??.*\$?\s*(\d+\.?\s?\d{2}))\n";
            var totalCostPattern3 = @"(?i)ount\s*(?:cost)?.*:??\s*\$?\s*(\d+\.?\s?\d{2})\n";

            List<string> totalCostPatterns = new List<string>()
            {
                totalCostPattern1, totalCostPattern2, totalCostPattern3
            };
            foreach (var pattern in totalCostPatterns)
            {
                var totalCostMatches = Regex.Matches(text, totalCostPattern1, RegexOptions.IgnoreCase);
                if (totalCostMatches.Count >0)
                {
                    foreach (var totalCostMatch in totalCostMatches)
                    {
                        var totalCost = ProcessString(totalCostMatch.ToString());
                        //decimal.TryParse(totalCostMatch.Groups[1].Value.Replace(" ", "."), out decimal totalCost);
                        Console.WriteLine("match found");
                        Console.WriteLine(totalCost);
                    }
                    break;
                }
                else
                {
                    Console.WriteLine("Unable to extract totalCost from the receipt. Please try again.");
                }
            }
            
            var datePattern1 = @"(?:date)?\s*:??\s*(19|20)\d\d[- /.](0[1-9]|1[012])[- /.](0[1-9]|[12][0-9]|3[01])"; // yyyy/mm/dd
            var datePattern2 = @"(?:date)?\s*:??\s*(0[1-9]|1[012])[- /.](0[1-9]|[12][0-9]|3[01])[- /.]((19|20)\d\d)"; // mm/dd/yyyy
            var datePattern3 = @"(?:date)?\s*:??\s*(0[1-9]|1[012])[- /.](0[1-9]|[12][0-9]|3[01])[- /.](\d\d)"; // mm/dd/yy
            var datePattern4 = @"(?:date)?\s*:??\s*(0[1-9]|[12][0-9]|3[01])[- /.](0[1-9]|1[012])[- /.]((19|20)\d\d)"; // dd/mm/yyyy

            List<string> datePatterns = new List<string>()
            { datePattern1, datePattern2, datePattern3, datePattern4 };

            foreach( var datePattern in datePatterns)
            {
                var dateMatch = Regex.Match(text, datePattern, RegexOptions.IgnoreCase);
                if (dateMatch.Success)
                {
                    DateTime.TryParse(dateMatch.Value.ToString(), out DateTime date);

                    Console.WriteLine("match found");
                    Console.WriteLine(date.ToShortDateString());
                    break;
                }
                else
                {
                    Console.WriteLine("Unable to extract date from the receipt. trying next method.");
                }
            }
            
            var tipPattern = @"(?i)tip.*:??\$?\s*(\d+\.?\s?\d{2})";
            var tipMatch = Regex.Match(text, tipPattern, RegexOptions.IgnoreCase);
            if (tipMatch.Success)
            {
                //decimal.TryParse(tipMatch.Groups[1].Value, out decimal tip);
                var tip = ProcessString(tipMatch.Value);
                Console.WriteLine("match found");
                Console.WriteLine(tip);
            }
            else
            {
                Console.WriteLine("Unable to extract tip from the receipt. Please try again.");
            }
        }
        static byte[] OpenCVProcessMethod(string filepath, int method = 0)
        {
            Mat result;
            switch (method)
            {
                case 1:
                    result = OpenCVprocessing_norotation(filepath);
                    Console.WriteLine("--- Process Method 1 ---");
                    break;
                case 2:
                    result = OpenCVprocessing_AdaptiveThreshhold(filepath);
                    Console.WriteLine("--- Process Method 2 ---");
                    break;
                default:
                    result = OpenCVprocessing_noprocessing(filepath);
                    Console.WriteLine("--- Process Method 0 ---");
                    break;

            }
            Bitmap bitmap = BitmapConverter.ToBitmap(result);
            byte[] bitmapBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                bitmapBytes = ms.ToArray();
            }
            return bitmapBytes;
        }
    }
}