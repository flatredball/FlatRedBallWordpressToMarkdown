using System;
using System.Collections.Generic;
using System.Text;

namespace FrbScraper
{
    public class HtmlMarkdownConverter
    {
        // NOTE: this method requires a path instead of a string because passing
        // HTML via commandline arguments does not work properly. Pandoc must
        // open, read, convert, and close the files. It's more IO ops but it
        // is the only reliable way to convert via Process Wrapping
        public static void ConvertHtmlToMarkdown(string htmlInputPath, string markdownOutputPath)
        {
            var processName = "pandoc";
            var arguments = $"-t gfm -o {markdownOutputPath} {htmlInputPath} --wrap=none";

            using (var pandoc = new ProcessWrapper(processName, arguments))
            {
                if(pandoc.Run())
                {
                    // execution worked, nothing to do here!
                }
                else
                {
                    throw new Exception($"Pandoc conversion failed: {pandoc.Error}");
                }
            }
        }

    }
}
