namespace PostsApi.Models
{
    public class NewPost
    {
        public int ParentId { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
    }
}
