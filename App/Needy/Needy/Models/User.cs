using pocketbase_csharp_sdk.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Needy.Models
{
    public class User : BaseModel
    {
        // The unique ID that PocketBase gives to each user (e.g., "k2g4ekefa...")
        public string Id { get; set; }

        // The complete name of the user
        [JsonPropertyName("name")]
        public string Name { get; set; }

        // Email (required for the login)
        [JsonPropertyName("email")]
        public string Email { get; set; }

        // The avatar: contains the name of the file (ex. "foto.jpg")
        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        // True/False. If the user have permission to interact
        [JsonPropertyName("verified")]
        public bool IsVerified { get; set; }

        // Reputation hour
        [JsonPropertyName("reputation_hour")]
        public int ReputationHour { get; set; }

        [JsonPropertyName("neighborhood")]
        public string Neighborhood { get; set; }

        public string Created {  get; set; }
        public string Updated { get; set; }
    }
}
