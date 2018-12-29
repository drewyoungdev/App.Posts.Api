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
        public int Score { get; set; }

        [JsonProperty(Order = 4)]
        public string Author { get; set; }

        [JsonProperty(Order = 5)]
        public DateTime CreateDate { get; set; }
    
        [JsonProperty(Order = 6)]
        public string Comment { get; set; }
        
        // Since we can limit the amount of replies returned per post, we still need to know the total amount.
        // e.g. if we only display the top 3 replies but there are 100 other replies, NumOfReplies would be 100 but Replies.Count would be 3.     
        // FE can take difference and show "Load "x" More Replies" where "x" can be limited.
        [JsonProperty(Order = 7)]
        public long NumOfReplies { get; set; }
        
        [JsonProperty(Order = 8)]
        public List<Post> Replies = new List<Post>();

        [JsonIgnore]
        public int Depth { get; set; }
    }
}
