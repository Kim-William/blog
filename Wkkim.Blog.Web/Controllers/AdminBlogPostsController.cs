
using System.Linq;

using DevExpress.Data.Utils;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting.Internal;

using Wkkim.Blog.Web.Models.Domain;
using Wkkim.Blog.Web.Models.ViewModels;
using Wkkim.Blog.Web.Repositories;

namespace Wkkim.Blog.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminBlogPostsController : Controller
    {
        private readonly ITagRepository tagRepository;
        private readonly IBlogPostRepository blogRepository;
        private readonly IImageRepository imageRepository;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment;

        public AdminBlogPostsController(ITagRepository tagRepository, IBlogPostRepository blogRepository, IImageRepository imageRepository, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment)
        {
            this.tagRepository = tagRepository;
            this.blogRepository = blogRepository;
            this.imageRepository = imageRepository;
            this.hostingEnvironment = hostingEnvironment;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            // get tags from repository

            var tags = await tagRepository.GetAllAsync();

            var model = new AddBlogPostRequest
            {
                Tags = tags.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add(AddBlogPostRequest addBlogPostRequest)
        {
            var blogPost = new BlogPost
            {
                Heading = addBlogPostRequest.Heading,
                PageTitle = addBlogPostRequest.PageTitle,
                Content = addBlogPostRequest.Content,
                ShortDescription = addBlogPostRequest.ShortDescription,
                FeaturedImageUrl = addBlogPostRequest.FeaturedImageUrl,
                UrlHandle = addBlogPostRequest.UrlHandle,
                PublishedDate = addBlogPostRequest.PublishedDate,
                Author = addBlogPostRequest.Author,
                Visible = addBlogPostRequest.Visible,
        };

            var idStr = blogPost.FeaturedImageUrl.Split('/').LastOrDefault().Split('.')[0];

            addBlogPostRequest.AttachedImageUrls = addBlogPostRequest.AttachedImageUrl.Split(",").Where(t => addBlogPostRequest.Content.Contains(t) && !string.IsNullOrWhiteSpace(t)).ToList().Append(idStr).ToArray();
            var deletedList = addBlogPostRequest.AttachedImageUrl.Split(",").Where(t => !addBlogPostRequest.Content.Contains(t) && !string.IsNullOrWhiteSpace(t)).ToArray();

            var webRootPath = hostingEnvironment.WebRootPath;
            var fileRoute = Path.Combine(webRootPath, "uploads");
            string[] files = System.IO.Directory.GetFiles(fileRoute);

            if (deletedList!=null && deletedList.Length > 0 )
            {
                for (int i = 0; i < deletedList.Length; i++)
                {
                    if (files.Any(t=>t.Contains(deletedList[i])))
                    {
                        var target = files.SingleOrDefault(t => t.Contains(deletedList[i]));
                        System.IO.File.Delete(Path.Combine(fileRoute, target));
                    }
                } 
            }

            var imgList = new List<Image>();

            if (addBlogPostRequest.AttachedImageUrls != null && addBlogPostRequest.AttachedImageUrls.Length > 0)
            {
                files = System.IO.Directory.GetFiles(fileRoute);

                try
                {
                    for (int i = 0; i < addBlogPostRequest.AttachedImageUrls.Length; i++)
                    {
                        if (files.Any(t => t.Contains(addBlogPostRequest.AttachedImageUrls[i])))
                        {
                            var target = files.SingleOrDefault(t => t.Contains(addBlogPostRequest.AttachedImageUrls[i]));

                            var img = new Image();
                            img.BlogPost = blogPost;
                            img.FilePath = Path.Combine(fileRoute, target);
                            img.Id = Guid.Parse(addBlogPostRequest.AttachedImageUrls[i]);

                            imgList.Add(img);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (addBlogPostRequest.AttachedImageUrls != null && addBlogPostRequest.AttachedImageUrls.Length > 0)
                    {
                        files = System.IO.Directory.GetFiles(fileRoute);

                        for (int i = 0; i < addBlogPostRequest.AttachedImageUrls.Length; i++)
                        {
                            if (files.Any(t => t.Contains(addBlogPostRequest.AttachedImageUrls[i])))
                            {
                                var target = files.SingleOrDefault(t => t.Contains(addBlogPostRequest.AttachedImageUrls[i]));
                                System.IO.File.Delete(Path.Combine(fileRoute, target));
                            }
                        }
                    }
                }
            }

            try
            {
                var selectedTags = new List<Tag>();
                foreach (var selectedTagId in addBlogPostRequest.SelectedTags)
                {
                    var selectedTagIdAsGuid = Guid.Parse(selectedTagId);
                    var existingTag = await tagRepository.GetAsync(selectedTagIdAsGuid);

                    if (existingTag != null)
                    {
                        selectedTags.Add(existingTag);
                    }
                }

                blogPost.Tags = selectedTags;

                blogPost.Images = imgList;
                await blogRepository.AddAsync(blogPost);

                foreach (var img in imgList)
                {
                    await imageRepository.AddAsync(img);
                }
            }
            catch (Exception ex)
            {
                if (addBlogPostRequest.AttachedImageUrls != null && addBlogPostRequest.AttachedImageUrls.Length > 0)
                {
                    files = System.IO.Directory.GetFiles(fileRoute);

                    for (int i = 0; i < addBlogPostRequest.AttachedImageUrls.Length; i++)
                    {
                        if (files.Any(t => t.Contains(addBlogPostRequest.AttachedImageUrls[i])))
                        {
                            var target = files.SingleOrDefault(t => t.Contains(addBlogPostRequest.AttachedImageUrls[i]));
                            System.IO.File.Delete(Path.Combine(fileRoute, target));

                            await imageRepository.DeleteAsync(Guid.Parse(addBlogPostRequest.AttachedImageUrls[i]));
                        }
                    }

                }
            }

            return RedirectToAction("Add");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var blogPosts = await blogRepository.GetAllAsync();

            return View(blogPosts);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var blogPost = await blogRepository.GetAsync(id);

            var tagsDomainModel = await tagRepository.GetAllAsync();

            if (blogPost != null)
            {
                var model = new EditBlogPostRequest
                {
                    Id = blogPost.Id,
                    Heading = blogPost.Heading,
                    PageTitle = blogPost.PageTitle,
                    Content = blogPost.Content,
                    Author = blogPost.Author,
                    FeaturedImageUrl = blogPost.FeaturedImageUrl,
                    UrlHandle = blogPost.UrlHandle,
                    ShortDescription = blogPost.ShortDescription,
                    PublishedDate = blogPost.PublishedDate,
                    Visible = blogPost.Visible,
                    Tags = tagsDomainModel.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }),
                    SelectedTags = blogPost.Tags.Select(t => t.Id.ToString()).ToArray()
                };

                return View(model);
            }

            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(EditBlogPostRequest editBlogPostRequest)
        {
            var blogPostDomainModel = new BlogPost
            {
                Id = editBlogPostRequest.Id,
                Heading = editBlogPostRequest.Heading,
                PageTitle = editBlogPostRequest.PageTitle,
                Content = editBlogPostRequest.Content,
                Author = editBlogPostRequest.Author,
                FeaturedImageUrl = editBlogPostRequest.FeaturedImageUrl,
                PublishedDate = editBlogPostRequest.PublishedDate,
                ShortDescription = editBlogPostRequest.ShortDescription,
                UrlHandle = editBlogPostRequest.UrlHandle,
                Visible = editBlogPostRequest.Visible,
            };

            var selectedTags = new List<Tag>();

            foreach (var selectedTag in editBlogPostRequest.SelectedTags)
            {
                if (Guid.TryParse(selectedTag, out var tag))
                {
                    var foundTag = await tagRepository.GetAsync(tag);
                    if (foundTag != null)
                    {
                        selectedTags.Add(foundTag);
                    }
                }
            }
            blogPostDomainModel.Tags = selectedTags;

            var updatedBlog = await blogRepository.UpdateAsync(blogPostDomainModel);

            if (updatedBlog != null)
            {
                // Show Success notification
                return RedirectToAction("List");
            }

            // Show error notification
            return RedirectToAction("Edit");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(EditBlogPostRequest editBlogPostRequest)
        {
            var deletedBlogPost = await blogRepository.DeleteAsync(editBlogPostRequest.Id);

            if (deletedBlogPost != null)
            {
                // Show success notification
                return RedirectToAction("List");
            }

            // Show error notification
            return RedirectToAction("Edit", new { id = editBlogPostRequest.Id });
        }
    }
}
