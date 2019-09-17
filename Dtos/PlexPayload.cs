using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AnilistPlexScrobbler.Dtos
{

    public class PlexPayload
    {
        public string Rating { get; set; }
        public string Event { get; set; }

        public bool User { get; set; }

        public bool Owner { get; set; }

        public Account Account { get; set; }

        public Server Server { get; set; }

        public Player Player { get; set; }

        public Metadata Metadata { get; set; }
    }

    public class Account
    {
        public int Id { get; set; }

        public Uri Thumb { get; set; }

        public string Title { get; set; }
    }

    public class Metadata
    {
        public string LibrarySectionType { get; set; }

        public string RatingKey { get; set; }

        public string Key { get; set; }

        public string ParentRatingKey { get; set; }

        public string GrandparentRatingKey { get; set; }

        public string Guid { get; set; }

        public int LibrarySectionId { get; set; }

        public string LibrarySectionTitle { get; set; }

        public string Type { get; set; }

        public string Title { get; set; }

        public string GrandparentKey { get; set; }

        public string ParentKey { get; set; }

        public string GrandparentTitle { get; set; }

        public string ParentTitle { get; set; }

        public string Summary { get; set; }

        public int Index { get; set; }

        public int? ParentIndex { get; set; }

        public int Year { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime LastViewedAt { get; set; }
        public int RatingCount { get; set; }

        public string Thumb { get; set; }

        public string Art { get; set; }

        public string ParentThumb { get; set; }

        public string GrandparentThumb { get; set; }

        public string GrandparentArt { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime AddedAt { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }
    }

    public class Player
    {
        public bool Local { get; set; }

        public string PublicAddress { get; set; }

        public string Title { get; set; }

        public string Uuid { get; set; }
    }

    public partial class Server
    {
        public string Title { get; set; }

        public string Uuid { get; set; }
    }
}
