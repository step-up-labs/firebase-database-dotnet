using Newtonsoft.Json;
using System;

namespace XamarinForms.Models
{
    public class PostComment
    {
        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}