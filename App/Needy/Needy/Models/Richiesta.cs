using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using pocketbase_csharp_sdk.Models;

namespace Needy.Models
{
    public class Richiesta : BaseModel
    {
        public string title { get; set; }
        public string description { get; set; }
        public string category { get; set; }
        public string status { get; set; }
        public double estimated_duration { get; set; }
        public string requester {  get; set; }
        public string assistant { get; set; }
    }
}
