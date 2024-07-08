using Wkkim.Blog.Web.Models.Domain;

namespace Wkkim.Blog.Web.Repositories
{
    public interface IImageRepository
    {
        Task<string> UploadAsync(IFormFile file);

        Task<Image> AddAsync(Image blogPost);

        Task<Image?> DeleteAsync(Guid id);
    }
}
