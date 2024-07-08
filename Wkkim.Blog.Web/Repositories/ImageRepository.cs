using Wkkim.Blog.Web.Data;
using Wkkim.Blog.Web.Models.Domain;

namespace Wkkim.Blog.Web.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly BlogDbContext blogDbContext;

        public ImageRepository(BlogDbContext blogDbContext)
        {
            this.blogDbContext = blogDbContext;
        }

        public async Task<Image> AddAsync(Image image)
        {
            await blogDbContext.AddAsync(image);
            return image;
        }

        public async Task<Image?> DeleteAsync(Guid id)
        {
            var existingImg = await blogDbContext.Images.FindAsync(id);

            if (existingImg != null)
            {
                System.IO.File.Delete(existingImg.FilePath);

                blogDbContext.Images.Remove(existingImg);
                await blogDbContext.SaveChangesAsync();
                return existingImg;
            }

            return null;
        }

        public Task<string> UploadAsync(IFormFile file)
        {
            throw new NotImplementedException();
        }
    }
}
