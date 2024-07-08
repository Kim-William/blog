namespace Wkkim.Blog.Web.Models.Domain
{
    public class Image
    {
        public Guid Id { get; set; }
        public string FilePath { get; set; }
        public BlogPost BlogPost { get; set; }
    }
}
