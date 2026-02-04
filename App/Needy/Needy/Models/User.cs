using System;
using System.Collections.Generic;
using System.Text;

namespace Needy.Models
{
    public class User
    {
        // The unique ID that PocketBase gives to each user (e.g., "k2g4ekefa...")
        public string Id { get; set; }

        // The complete name of the user
        public string Name { get; set; }

        // Email (required for the login)
        public string Email { get; set; }

        // The avatar: contains the name of the file (ex. "foto.jpg")
        public string Avatar { get; set; }

        // True/False. If the user have permission to interact
        public bool IsVerified { get; set; }
        
        // Reputation hour
        public int ReputationHour { get; set; }

        public string Neighborhood { get; set; }

        public string Created {  get; set; }
        public string Updated { get; set; }
    }
}
