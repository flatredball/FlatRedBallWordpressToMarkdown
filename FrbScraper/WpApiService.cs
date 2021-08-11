using FrbScraper.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft;
using System.Threading;
using System.Globalization;
using System.IO;
using CsvHelper;
using System.Text.RegularExpressions;
using System.Linq;

namespace FrbScraper
{
    public class WpApiService
    {
        const string CsvName = "pageList.csv";
        const string HtmlDirectoryName = "html";
        const string MediaDirectoryName = "media";
        const string MarkdownDirectoryName = "";

        private static WpApiService instance;
        private WebClient webClient;
        private bool initialized = false;
        int pageCount = 0;


        public static WpApiService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new WpApiService();
                }
                return instance;
            }
        }

        public string SiteUrl { get; set; }

        public string OutputPath { get; set; }

        public string HtmlPagePath
        {
            get
            {
                return Path.Combine(OutputPath, HtmlDirectoryName);
            }
        }

        public string MarkdownPagePath
        {
            get
            {
                return Path.Combine(OutputPath, MarkdownDirectoryName);
            }
        }

        public string MediaPath
        {
            get
            {
                return Path.Combine(OutputPath, MediaDirectoryName);
            }
        }

        public string CsvPath
        {
            get
            {
                return Path.Combine(OutputPath, CsvName);
            }
        }

        public string ApiUrl
        {
            get
            {
                return $"https://{SiteUrl}/wp-json";
            }
        }



        private WpApiService()
        {
            webClient = new WebClient();
        }

        public void Initialize(string siteUrl, string outputPath)
        {
            if(siteUrl.Contains("https://") || siteUrl.Contains("http://"))
            {
                throw new Exception("SiteUrl should not include http://");
            }

            SiteUrl = siteUrl;
            OutputPath = outputPath;

            if (!Directory.Exists(HtmlPagePath))
            {
                Directory.CreateDirectory(HtmlPagePath);
            }

            if (!Directory.Exists(MediaPath))
            {
                Directory.CreateDirectory(MediaPath);
            }

            if(!Directory.Exists(MarkdownPagePath))
            {
                Directory.CreateDirectory(MarkdownPagePath);
            }

            initialized = true;
        }

        public void ConvertPages()
        {
            CheckInit();

            var url = $"{ApiUrl}/wp/v2/pages?page=";
            using (var writer = new StreamWriter(CsvPath, true))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    for (var pg = 1; pg < int.MaxValue; pg++)
                    {
                        var reqUrl = url + pg;
                        var content = webClient.DownloadString(reqUrl);
                        var pages = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WpPost>>(content);

                        // EARLY OUT: no more pages!
                        if (pages.Count == 0)
                        {
                            break;
                        }

                        csv.WriteRecords(pages);
                        writer.Flush();

                        foreach (var page in pages)
                        {
                            CreatePage(page);
                        }
                    }
                }
            }
        }



        private void CreatePage(WpPost page)
        {
            var basePath = GetBasePath(page.Link, false);
            var htmlPath = CreateDirectoriesRecursive(basePath, HtmlPagePath);
            var markdownPath = CreateDirectoriesRecursive(basePath, MarkdownPagePath);
            var filename = page.Slug;

            // this page is the main page for the directory
            if(htmlPath.EndsWith(page.Slug))
            {
                filename = "index";
            }

            // put the full path together
            htmlPath = Path.Combine(htmlPath, filename + ".html");
            markdownPath = Path.Combine(markdownPath, filename + ".md");

            var html = page.Content.ToString();
            html = CleanHtml(html);
            html = MakeMediaFilesLocal(html);
            html = MakeLinksLocal(html);

            // create temp html file
            File.WriteAllText(htmlPath, html);

            // create markdown page
            HtmlMarkdownConverter.ConvertHtmlToMarkdown(htmlPath, markdownPath);

            pageCount++;
            Console.WriteLine($"{pageCount} - Created {markdownPath}");

            // delete html page
            //File.Delete(htmlPath);
        }

        private string CleanHtml(string html)
        {
            // strip span tags
            html = Regex.Replace(html, "<[Ss][Pp][Aa][Nn][^>]*>", "");

            return html;
        }

        private string MakeMediaFilesLocal(string html)
        {
            Regex images = new Regex("<[Ii][Mm][Gg][^>]+src=[\"']([^\"'>]+)[\"'][^>]*>");
            foreach (Match match in images.Matches(html))
            {
                var img = match.Groups[0].Value;
                var src = match.Groups[1].Value;
                var fullLocalPath = FetchMediaItem(src);
                var relativePath = $@"/{MediaDirectoryName}" + fullLocalPath.Replace(MediaPath, "");
                var imgRewrite = $"<img src=\"{relativePath.Replace(@"\", "/")}\" />";
                html = html.Replace(img, imgRewrite);
            }
            return html;
        }

        private string MakeLinksLocal(string html)
        {
            Regex anchors = new Regex("<[Aa][^>]+href=[\"']([^\"']+)[\"'][^>]*>");
            foreach (Match match in anchors.Matches(html))
            {
                var anchor = match.Groups[0].Value;
                var href = match.Groups[1].Value;
                // make absolute paths relative
                href = href.Replace($"https://{SiteUrl}", "");
                href = href.Replace($"http://{SiteUrl}", "");
                var anchorRewrite = $"<a src=\"{href}\">";
                html = html.Replace(anchor, anchorRewrite);
            }
            return html;
        }

        private string FetchMediaItem(string url)
        {
            // EARLY OUT: not a site media asset, just return the url
            if(!IsSitePath(url))
            {
                return url;
            }
            var dirPath = GetBasePath(url);
            dirPath = CreateDirectoriesRecursive(dirPath, OutputPath);
            var filename = Path.GetFileName(url);
            var localPath = Path.Combine(dirPath, filename);
            webClient.DownloadFile(url, localPath);
            return localPath;
        }

        private bool IsSitePath(string url)
        {
            return url.Contains(SiteUrl);
        }

        private string GetBasePath(string url, bool mediaPath = true)
        {
            var basePath = url;

            // NOTE: does this deal with subdomains like files.site.com?
            basePath = Regex.Replace(basePath, "[A-Za-z]+://", "");
            basePath = basePath.Replace(SiteUrl, "");
            basePath = basePath.Replace("wp-content/uploads/", "");

            if(mediaPath)
            {
                basePath = MediaDirectoryName + basePath;
            }

            return basePath;
        }

        private string CreateDirectoriesRecursive(string filePath, string baseDirectory)
        {
            // make sure provided path is relative
            var testPath = Regex.Replace(filePath, "[A-Za-z]+://", "");
            if(testPath != filePath)
            {
                throw new Exception($"{filePath} should be a relative path to be created in {baseDirectory}");
            }

            // remove filename if it exists
            var fileName = Path.GetFileName(filePath);
            if(!string.IsNullOrEmpty(fileName))
            {
                filePath = filePath.Replace(fileName, "");
            }

            var directories = filePath.Split("/");
            var builtPath = baseDirectory;
            for(var i = 0; i < directories.Length; i++)
            {
                var dir = directories[i];
                if(string.IsNullOrWhiteSpace(dir))
                {
                    continue;
                }
                builtPath = Path.Combine(builtPath, dir);
                if(!Directory.Exists(builtPath))
                {
                    Directory.CreateDirectory(builtPath);
                }
            }
            return builtPath;
        }

        private void CheckInit()
        {
            if (!initialized)
            {
                throw new Exception("Attempted to use service before initializing.");
            }
        }
    }
}
