using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PostsApi.Models
{
    public class Post
    {
        [JsonProperty(Order = 1)]
        public int Id { get; set; }
        
        [JsonProperty(Order = 2)]
        public int ParentId { get; set; }

        [JsonProperty(Order = 3)]
        public int Upvotes { get; set; }

        [JsonProperty(Order = 4)]
        public int Downvotes { get; set; }

        [JsonProperty(Order = 5)]
        public int Score { get; set; }

        [JsonProperty(Order = 6)]
        public string Author { get; set; }

        [JsonProperty(Order = 7)]
        public DateTime CreateDate { get; set; }
    
        [JsonProperty(Order = 8)]
        public string Body { get; set; }

        [JsonProperty(Order = 9)]
        public int Depth { get; set; }
        
        [JsonProperty(Order = 10)]
        public long NumOfReplies { get; set; }
        
        [JsonProperty(Order = 11)]
        public long NumOfHiddenReplies { get { return NumOfReplies - Replies.Count; } }

        [JsonProperty(Order = 12)]
        public bool MustContinueInNewThread { get; set; }
        
        [JsonProperty(Order = 13)]
        public List<Post> Replies = new List<Post>();
    }
}
