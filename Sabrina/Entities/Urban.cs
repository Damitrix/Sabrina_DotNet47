namespace Sabrina.Entities
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using J = Newtonsoft.Json.JsonPropertyAttribute;
    using R = Newtonsoft.Json.Required;
    using N = Newtonsoft.Json.NullValueHandling;

    public partial class Urban
    {
        [J("list")] public List<List> List { get; set; }
    }

    public partial class List
    {
        [J("definition")] public string Definition { get; set; }
        [J("permalink")] public Uri Permalink { get; set; }
        [J("thumbs_up")] public long ThumbsUp { get; set; }
        [J("sound_urls")] public List<object> SoundUrls { get; set; }
        [J("author")] public string Author { get; set; }
        [J("word")] public string Word { get; set; }
        [J("defid")] public long Defid { get; set; }
        [J("current_vote")] public string CurrentVote { get; set; }
        [J("written_on")] public DateTimeOffset WrittenOn { get; set; }
        [J("example")] public string Example { get; set; }
        [J("thumbs_down")] public long ThumbsDown { get; set; }
    }

    public partial class Urban
    {
        public static Urban FromJson(string json) => JsonConvert.DeserializeObject<Urban>(json, Sabrina.Entities.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Urban self) => JsonConvert.SerializeObject(self, Sabrina.Entities.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
