using System;

namespace FrbScraper
{
    // TODOS:
    // - preserve directory structure for HTML files
    // - make image paths absolute
    // - convert html to markdown
    // - figure out how to disable crayon highlighting

    class Program
    {
        const string LocalPath = @"PUT YOUR LOCAL PATH HERE";

        static void Main(string[] args)
        {
            WpApiService.Instance.Initialize("flatredball.com", LocalPath);
            WpApiService.Instance.ConvertPages();
        }
    }
}
