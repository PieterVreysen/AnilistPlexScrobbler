using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AnilistPlexScrobbler.Dtos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AnilistPlexScrobbler.Services
{
    public class AnilistService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _token;

        public AnilistService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _token = config.GetValue<string>("token");
        }

        public async Task Sync(PlexPayload payload)
        {
            var anime = await SearchAnilist(payload);
            if (anime == null) return;
            
            await UpdateAniList(payload, anime);
        }

        private async Task UpdateAniList(PlexPayload payload, Media anime)
        {
            var userId = await GetUser();
            if (!userId.HasValue) return;

            var mediaListMatch = await SearchMediaList(userId.Value, anime);

            if (string.IsNullOrEmpty(payload.Rating) && mediaListMatch != null && mediaListMatch.Status == MediaListStatus.COMPLETED) return;
            await SaveMediaList(payload, anime);
        }

        private async Task SaveMediaList(PlexPayload payload, Media anime)
        {
            var mutation = @"
            mutation($mediaId: Int, $status: MediaListStatus, $progress: Int, $score: Float, $startedAt: FuzzyDateInput, $completedAt: FuzzyDateInput){ 
                SaveMediaListEntry (mediaId: $mediaId, status: $status, progress: $progress, score: $score, startedAt: $startedAt, completedAt: $completedAt)
                { 
                    id 
                    status 
                } 
            }
            ";
            var variables = new QueryVariables();
            variables.MediaId = anime.Id;

            if (!string.IsNullOrEmpty(payload.Rating))
            {
                variables.Score = int.Parse(payload.Rating);
            }
            else
            {
                var isMovie = payload.Metadata.Type == "movie";
                var isCompleted = isMovie ? true : payload.Metadata.Index == anime.Episodes;

                variables.Status = isCompleted ? MediaListStatus.COMPLETED : MediaListStatus.CURRENT;
                if (!isCompleted)
                {
                    variables.Progress = payload.Metadata.Index;
                }
            }

            var queryBody = new QueryBody(mutation, variables);

            await QueryAnilist(queryBody);
        }

        private async Task<Media> SearchAnilist(PlexPayload payload)
        {
            var query = @"
            query($search:String, $seasonYear: Int, $format: MediaFormat){
                Page (page: 1, perPage: 1) {
                    pageInfo {
                    total
                    currentPage
                    lastPage
                    hasNextPage
                    perPage
                    }
                    media(search:$search, seasonYear:$seasonYear, format: $format, type:ANIME){
                    id
                    title {
                        romaji
                    }
                    episodes
                    }
                }
            }
            ";

            var variables = new QueryVariables
            {
                Search = payload.Metadata.GrandparentTitle ?? payload.Metadata.ParentTitle ?? payload.Metadata.Title,
                SeasonYear = payload.Metadata.Year,
                Format = payload.Metadata.Type == "movie" ? MediaFormat.MOVIE : MediaFormat.TV
            };
            var queryBody = new QueryBody(query, variables);

            var responseData = await QueryAnilist(queryBody);

            var anilistReturnData = AnlistReturnData.FromJson(responseData);

            return anilistReturnData.Data.Page?.Media?.FirstOrDefault();
        }

        private async Task<MediaList> SearchMediaList(int userId, Media anime)
        {
            var query = @"
            query($userId: Int, $mediaId: Int){
                Page (page: 1, perPage: 50) {
                    pageInfo {
                        total
                        currentPage
                        lastPage
                        hasNextPage
                        perPage
                    }
                    mediaList(userId: $userId, mediaId : $mediaId, type:ANIME){
                        id
                        status
                    }
                }
            }
            ";
            var variables = new QueryVariables
            {
                UserId = userId,
                MediaId = anime.Id
            };
            var queryBody = new QueryBody(query, variables);

            var responseData = await QueryAnilist(queryBody);

            var anilistReturnData = AnlistReturnData.FromJson(responseData);

            return anilistReturnData.Data.Page?.MediaList?.FirstOrDefault();
        }

        private async Task<int?> GetUser()
        {
            var query = @"
            query{
                Viewer{
                    id
                }
            }
            ";

            var queryBody = new QueryBody(query);
            var responseData = await QueryAnilist(queryBody);

            var anilistReturnData = AnlistReturnData.FromJson(responseData);

            return anilistReturnData.Data.Viewer?.Id;
        }

        private async Task<string> QueryAnilist(QueryBody body)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(CreateQueryRequest(body));

            if (!response.IsSuccessStatusCode && response.Headers.RetryAfter != null && response.Headers.RetryAfter.Delta.HasValue)
            {
                await Task.Delay((int)response.Headers.RetryAfter.Delta.Value.TotalMilliseconds);
                response = await client.SendAsync(CreateQueryRequest(body));
            }

            var responseData = await response.Content.ReadAsStringAsync();

            return responseData;
        }

        private HttpRequestMessage CreateQueryRequest(QueryBody queryBody)
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
            "https://graphql.anilist.co");
            request.Headers.Add("Authorization", $"Bearer {_token}");
            request.Headers.Add("Accept", "application/json");

            var body = JsonConvert.SerializeObject(queryBody, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, ContractResolver = new CamelCasePropertyNamesContractResolver() });
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            return request;
        }

        private class QueryBody
        {
            public QueryBody(string query, QueryVariables variables = null)
            {
                Query = query;
                Variables = variables;
            }
            public string Query { get; set; }
            public QueryVariables Variables { get; set; }
        }

        private class QueryVariables
        {
            public string Search { get; set; }
            public int? UserId { get; set; }
            public int? MediaId { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            public MediaListStatus? Status { get; set; }
            public int? Progress { get; set; }
            public int? Score { get; set; }
            public int? SeasonYear { get; set; }
            public FuzzyDateInput StartedAt { get; set; }
            public FuzzyDateInput CompletedAt { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            public MediaFormat? Format { get; set; }
        }

        private class FuzzyDateInput
        {
            public FuzzyDateInput(int year, int month, int day)
            {
                Year = year;
                Month = month;
                Day = day;
            }
            public int Year { get; set; }
            public int Month { get; set; }
            public int Day { get; set; }
        }
    }
}