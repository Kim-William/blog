using System.Collections;
using System.Formats.Tar;
using System.Net;

using Azure.Core;

using DevExpress.Data.Helpers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using Wkkim.Blog.Web.Models.Domain;
using Wkkim.Blog.Web.Models.ViewModels;
using Wkkim.Blog.Web.Repositories;

namespace Wkkim.Blog.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageRepository imageRepository;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment;

        public ImagesController(IImageRepository imageRepository, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment)
        {
            this.imageRepository = imageRepository;
            this.hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        public async Task<IActionResult> UploadAsync(IFormFile file)
        {
            // call a repository

            try
            {
                var webRootPath = hostingEnvironment.WebRootPath;
                var fileRoute = Path.Combine(webRootPath, "uploads");
                var extension = System.IO.Path.GetExtension(file.FileName);
                var id = Guid.NewGuid();
                var name = id.ToString() + extension;
                var link = Path.Combine(fileRoute, name);
                var mimeType = file.ContentType;
                var dir = new FileInfo(fileRoute);
                dir.Directory.Create();

                // Basic validation on mime types and file extension
                string[] imageMimetypes = { "image/gif", "image/jpeg", "image/pjpeg", "image/x-png", "image/png", "image/svg+xml" };
                string[] imageExt = { ".gif", ".jpeg", ".jpg", ".png", ".svg", ".blob" };

                if (Array.IndexOf(imageMimetypes, mimeType) >= 0 && (Array.IndexOf(imageExt, extension) >= 0))
                {
                    // Copy contents to memory stream.
                    Stream stream;
                    stream = new MemoryStream();
                    file.CopyTo(stream);
                    stream.Position = 0;
                    String serverPath = link;
                    System.IO.Directory.CreateDirectory(fileRoute);
                    // Save the file
                    using (FileStream writerFileStream = System.IO.File.Create(serverPath))
                    {
                        await stream.CopyToAsync(writerFileStream);
                        writerFileStream.Dispose();
                    }

                    return new JsonResult(new { link = "/uploads/" + name , id});
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}

