using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AnilistPlexScrobbler.Dtos
{
    public partial class AnlistReturnData
    {
        public Data Data { get; set; }
    }

    public class Data
    {
        public Page Page { get; set; }
        public Viewer Viewer { get; set; }
    }

    public class Viewer
    {
        public int Id { get; set; }
    }

    public class Page
    {
        public PageInfo PageInfo { get; set; }

        public Media[] Media { get; set; }

        public List<MediaList> MediaList { get; set; }
    }

    public class MediaList
    {
        public int Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MediaListStatus Status { get; set; }
    }

    public class Media
    {
        public int Id { get; set; }
        public int? Episodes { get; set; }
        public Title Title { get; set; }
    }

    public class Title
    {
        public string Romaji { get; set; }
    }

    public class PageInfo
    {
        public int Total { get; set; }

        public int CurrentPage { get; set; }

        public int LastPage { get; set; }

        public bool HasNextPage { get; set; }

        public int PerPage { get; set; }
    }

    public enum MediaListStatus
    {
        CURRENT,
        PLANNING,
        COMPLETED,
        DROPPED,
        PAUSED,
        REPEATING
    }

    public enum MediaFormat { 
        TV,
        TV_SHORT,
        MOVIE,
        SPECIAL,
        OVA,
        ONA,
        MUSIC,
        MANGA,
        NOVEL,
        ONE_SHOT
    }

    public partial class AnlistReturnData
    {
        public static AnlistReturnData FromJson(string json) => JsonConvert.DeserializeObject<AnlistReturnData>(json, AnilistReturnDataConverter.Settings);
    }

    public static class AnilistReturnDataSerialize
    {
        public static string ToJson(this AnlistReturnData self) => JsonConvert.SerializeObject(self, AnilistReturnDataConverter.Settings);
    }

    internal static class AnilistReturnDataConverter
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