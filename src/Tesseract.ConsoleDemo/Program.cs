using System;
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
            var testImagePath = "C:\\Users\\swei\\Downloads\\3.jpg";
            if (args.Length > 0)
            {
                testImagePath = args[0];
            }
            //Mat result = OpenCVprocessing(testImagePath);
            Mat result = OpenCVprocessing_norotation(testImagePath);
            Bitmap bitmap = BitmapConverter.ToBitmap(result);
            byte[] bitmapBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                bitmapBytes = ms.ToArray();
            }
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

                            Console.WriteLine("Text (GetText): \r\n{0}", text);

                            // Extract receipt information using regex patterns
                            
                            var totalCostPattern = @"(?i)total\s*(?:cost)?.*:??\s*\$?\s*(\d+\.?\s?\d{2})\n";
                            var totalCostMatch = Regex.Match(text, totalCostPattern, RegexOptions.IgnoreCase);
                            if (totalCostMatch.Success)
                            {
                                var totalCost = ProcessString(totalCostMatch.Value);
                                //decimal.TryParse(totalCostMatch.Groups[1].Value.Replace(" ", "."), out decimal totalCost);
                                Console.WriteLine("match found");
                                Console.WriteLine(totalCost);
                            }
                            else
                            {
                                Console.WriteLine("Unable to extract totalCost from the receipt. Please try again.");
                            }
                            var datePattern = @"(?:date)?\s*:??\s*(19|20)\d\d[- /.](0[1-9]|1[012])[- /.](0[1-9]|[12][0-9]|3[01])";
                            var dateMatch = Regex.Match(text, datePattern, RegexOptions.IgnoreCase);

                            if (dateMatch.Success)
                            {
                                DateTime.TryParse(dateMatch.Value.ToString(), out DateTime date) ;

                                Console.WriteLine("match found");
                                Console.WriteLine(date.ToShortDateString());
                            }
                            else
                            {                                
                                Console.WriteLine("Unable to extract date from the receipt. Please try again.");
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
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Console.WriteLine("Unexpected Error: " + e.Message);
                Console.WriteLine("Details: ");
                Console.WriteLine(e.ToString());
            }
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
    }
}