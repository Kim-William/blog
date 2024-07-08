
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

using Wkkim.Blog.Web.Models.Domain;

namespace Wkkim.Blog.Web.Repositories
{
    public class CloudinaryImageRepository : IImageRepository
    {
        private readonly IConfiguration configuration;
        private readonly Account account;

        public CloudinaryImageRepository(IConfiguration configuration)
        {
            this.configuration = configuration;
            account = new Account(
                configuration.GetSection("Cloudinary")["CloudName"],
                configuration.GetSection("Cloudinary")["ApiKey"],
                configuration.GetSection("Cloudinary")["ApiSecret"]);
        }

        public Task<Image> AddAsync(Image blogPost)
        {
            throw new NotImplementedException();
        }

        public Task<Image?> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            var client = new Cloudinary(account);
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                DisplayName = file.FileName,
                Transformation = new Transformation().Width(855).Crop("scale").Chain().FetchFormat("auto")
            };

            var uploadResult = await client.UploadAsync(uploadParams);

            if (uploadResult != null && uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return uploadResult.SecureUri.ToString();
            }

            return null;
        }
    }
}
