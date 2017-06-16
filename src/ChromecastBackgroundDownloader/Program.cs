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
        public static void Main(string[] args)
        {
            var backgroundUrls = parseBackgrounds();
            downloadBackgrounds(backgroundUrls).Wait();
        }

        private static IDictionary<string, string> parseBackgrounds()
        {
            var regex = @"!\[\]\((http[s]?:\/\/[\w.]*[\/\w-]*\/([\S]*))\)";
            return File.ReadAllLines("../../README.md")
                .Select(x => Regex.Match(x, regex))
                .ToDictionary(url => url.Groups[2].Value, fileName => fileName.Groups[1].Value);
        }

        private static async Task downloadBackgrounds(IDictionary<string, string> backgroundUrls)
        {
            var total = backgroundUrls.Count;
            var index = 1;
            foreach (var backgroundUrl in backgroundUrls)
            {
                await downloadBackgrounds(backgroundUrl.Value, backgroundUrl.Key, index, total);
                index++;
            }
        }

        private static async Task downloadBackgrounds(string url, string fileName, int index, int total)
        {
            var path = $@"\\vmware-host\Shared Folders\miccar\Pictures\Chromecast Backgrounds\{fileName}";
            if (File.Exists(path))
            {
                Console.WriteLine($"[{index}/{total}] {fileName} already exists. Skipping");
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
                Console.WriteLine($"[{index}/{total}] Finished downloading {fileName}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{index}/{total}] Error occured while downloading {fileName}\r\n    {e.Message}");
            }
        }
    }
}
