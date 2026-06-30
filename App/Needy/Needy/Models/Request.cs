using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using pocketbase_csharp_sdk.Models;

namespace Needy.Models
{
#pragma warning disable IDE1006
    public class Request : BaseModel
    {
        [JsonPropertyName("title")]
        public string title { get; set; }

        [JsonPropertyName("description")]
        public string description { get; set; }

        [JsonPropertyName("category")]
        public string category { get; set; }

        [JsonPropertyName("status")]
        public string status { get; set; }

        [JsonPropertyName("estimated_duration")]
        public double estimated_duration { get; set; }

        [JsonPropertyName("requester")]
        public string requester {  get; set; }

        [JsonPropertyName("assistant")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string assistant { get; set; }

        [JsonPropertyName("candidates")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> candidates {  get; set; } = new List<string>();

        [JsonPropertyName("candidate_messages")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string> candidate_messages { get; set; } = new Dictionary<string, string>();

        [JsonIgnore]
        public string MyRule { get; set; }

        [JsonIgnore]
        public string VisualState { get; set; }

        [JsonIgnore]
        public string StateColor {  get; set; }

        [JsonIgnore]
        public bool IsMyRequest {  get; set; }
    }

#pragma warning restore IDE1006
}
