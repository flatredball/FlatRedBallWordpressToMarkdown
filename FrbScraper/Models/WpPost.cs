using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrbScraper.Models
{
    public class WpPost
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("date_gmt")]
        public DateTime CreatedGmt { get; set; }

        [JsonProperty("modified_gmt")]
        public DateTime ModifiedGmt { get; set; }
        [JsonProperty("guid")]
        public RenderedContent Guid { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }
        [JsonProperty("title")]
        public RenderedContent Title { get; set; }
        [JsonProperty("parent")]
        public int ParentId { get; set; }
        [CsvHelper.Configuration.Attributes.Ignore()]
        public RenderedContent Content { get; set; }


        //[JsonIgnore]
        //public static IEnumerable<string> ListableHeaders
        //{
        //    get
        //    {
        //        yield return "Id";
        //        yield return "DateCreated";
        //        yield return "DateModified";
        //        yield return "Guid";
        //        yield return "Slug";
        //        yield return "Status";
        //        yield return "Type";
        //        yield return "Link";
        //        yield return "Title";
        //    }
        //}

        //[JsonIgnore]
        //public IEnumerable<string> ListableValues
        //{
        //    get
        //    {
        //        yield return Id.ToString();
        //        yield return CreatedGmt.ToString();
        //        yield return ModifiedGmt.ToString();
        //        yield return Guid.ToString();
        //        yield return Slug;
        //        yield return Status;
        //        yield return Type;
        //        yield return Link;
        //        yield return Title.ToString();
        //    }
        //}
    }

    public class RenderedContent
    {
        [JsonProperty("rendered")]
        public string Rendered { get; set; }

        public override string ToString()
        {
            return Rendered;
        }
    }
}
