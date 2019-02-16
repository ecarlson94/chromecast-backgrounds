using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChromecastBackgroundDownloader
{
    public class Program
    {
        private const string DestinationFolder = @"C:\Users\ecarl\Pictures\Chrome Backgrounds";

        public static void Main(string[] args)
        {
            var backgroundTasks = parseBackgrounds().Select(backgroundUrl =>
                downloadBackground(backgroundUrl.Value, backgroundUrl.Key));

            Task.WhenAll(backgroundTasks)
                .Wait();
        }

        private static IDictionary<string, string> parseBackgrounds()
        {
            var regex = @"!\[\]\((http[s]?:\/\/[\w.]*[\/\w-]*\/([\S]*))\)";
            return File.ReadAllLines("../../README.md")
                .Select(x => Regex.Match(x, regex))
                .ToDictionary(url => url.Groups[2].Value, fileName => fileName.Groups[1].Value);
        }

        private static async Task downloadBackground(string url, string fileName)
        {
            var path = $@"{DestinationFolder}\{fileName}";
            if (File.Exists(path))
            {
                Console.WriteLine($"{fileName} already exists. Skipping");
                return;
            }

            try
            {
                using (var client = new HttpClient())
                using (var contentStream = await client.GetStreamAsync(url))
                using (
                    var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1048576,
                        true))
                {
                    await contentStream.CopyToAsync(fileStream);
                }
                Console.WriteLine($"Finished downloading {fileName}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured while downloading {fileName}\r\n    {e.Message}");
            }
        }
    }
}
