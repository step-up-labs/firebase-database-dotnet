using Newtonsoft.Json;
using System;

namespace XamarinForms.Models
{
    public class Post
    {
        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }
}