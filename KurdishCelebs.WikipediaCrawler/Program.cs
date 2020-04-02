using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace KurdishCelebs.WikipediaCrawler
{
    class Image
    {
        public string Url { get; set; }
        public string Type { get; set; }
    }

    class Celebrity
    {
        public string EnglishName { get; set; }
        public string KurdishName { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var celebs = GetCelebNames();
            await DownloadPictures("data/images", celebs);
        }

        private static string Sanitize(string text)
        {
            return text.ToLowerInvariant()
                       .Replace('ş', 's')
                       .Replace('ğ', 'g')
                       .Replace('ç', 'c')
                       .Replace('ı', 'i')
                       .Replace('ê', 'e')
                       .Replace('î', 'i')
                       .Replace(' ', '_');
        }

        private static async Task DownloadPictures(string folder, IEnumerable<Celebrity> celebrities)
        {
            if (Directory.Exists(folder) == false)
            {
                Directory.CreateDirectory(folder);
            }

            foreach (var celeb in celebrities)
            {
                try
                {
                    Console.WriteLine($"Downloading images for {celeb.EnglishName}...");
                    var images = await SearchForImage(celeb.KurdishName);

                    var celebFolder = Path.Combine(folder, Sanitize(celeb.EnglishName));
                    if (Directory.Exists(celebFolder) == false)
                    {
                        Directory.CreateDirectory(celebFolder);
                    }

                    var tasks = images.Select((im, i) =>
                    {
                        var fullPath = Path.Combine(celebFolder, $"{i}.{im.Type}");
                        return DownloadFile(im.Url, fullPath);
                    }).ToArray();

                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private static async Task DownloadFile(string url, string path)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var content = await client.GetStreamAsync(url);
                    using (var file = File.OpenWrite(path))
                    {
                        await content.CopyToAsync(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task<List<Image>> SearchForImage(string name)
        {
            const string subscriptionKey = "4d0f2b76e8cc41a79befcd8d090bc46e";
            const string uriBase = "https://api.cognitive.microsoft.com/bing/v7.0/images/search";

            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Request parameters
            queryString["q"] = name;
            queryString["count"] = "10";
            queryString["offset"] = "0";
            queryString["mkt"] = "en-us";
            queryString["safeSearch"] = "Moderate";
            queryString["size"] = "large";
            //  queryString["imageContent"] = "Face";

            var response = await client.GetStringAsync(uriBase + "?" + queryString);

            var parsed = JObject.Parse(response);
            var parsedImages = parsed["value"] as JArray;

            var list = new List<Image>();

            foreach (JObject parsedImage in parsedImages)
            {
                var image = new Image
                {
                    Url = parsedImage.Value<string>("contentUrl"),
                    Type = parsedImage.Value<string>("encodingFormat"),
                };

                list.Add(image);
            }

            return list;
        }

        private static List<Celebrity> GetCelebNames()
        {
            return File.ReadAllLines("data/celebs.txt")
                       .Skip(1) // skip headers
                       .Select(l => l.Split('\t'))
                       .Select(parts => new Celebrity
                       {
                           EnglishName = parts[0],
                           KurdishName = parts[1]
                       }).ToList();
        }

        //private static Dictionary<string, List<string>> GetNames(string html)
        //{
        //    var dict = new Dictionary<string, List<string>>();
        //    var doc = new HtmlDocument();
        //    doc.LoadHtml(html);

        //    var uls = doc.DocumentNode.Descendants("ul").ToList();

        //    foreach (var ul in uls)
        //    {
        //        var h2 = ul.ParentNode?.PreviousSibling?.PreviousSibling;
        //        if (h2 != null && h2.Name == "h2")
        //        {
        //            var list = ul.Elements("li").Select(e => e.InnerText).ToList();
        //            var h2Content = h2.InnerText.Replace("[edit]", "").ToLowerInvariant().Trim();
        //            dict[h2Content] = list;
        //        }
        //    }

        //    return dict;
        //}

        private static async Task<string> DownloadPage(string url)
        {
            using (var client = new HttpClient())
            {
                var userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.134 Safari/537.36";
                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

                return await client.GetStringAsync(url);
            }
        }
    }
}
