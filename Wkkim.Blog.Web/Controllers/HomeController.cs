using System.Diagnostics;
using System.Linq;

using Microsoft.AspNetCore.Mvc;

using Wkkim.Blog.Web.Data;
using Wkkim.Blog.Web.Models;
using Wkkim.Blog.Web.Models.Domain;
using Wkkim.Blog.Web.Models.ViewModels;
using Wkkim.Blog.Web.Repositories;

namespace Wkkim.Blog.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBlogPostRepository blogPostRepository;
        private readonly ITagRepository tagRepository;

        public HomeController(ILogger<HomeController> logger,
            IBlogPostRepository blogPostRepository,
            ITagRepository tagRepository)
        {
            _logger = logger;
            this.blogPostRepository = blogPostRepository;
            this.tagRepository = tagRepository;
        }

        public async Task<IActionResult> Index()
        {
            // getting all blogs
            var blogPosts = await blogPostRepository.GetAllAsync();

            // get all tags
            var tags = await tagRepository.GetAllAsync();

            var model = new HomeViewModel
            {
                BlogPosts = blogPosts,
                Tags = tags
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> PostsByTag(string[] tagIds)
        {
            var tagids = new Guid[tagIds.Length];

            for (int i = 0; i < tagIds.Length; i++)
            {
                tagids[i] = Guid.Parse(tagIds[i]);
            }

            // getting all blogs
            var blogPosts = await blogPostRepository.GetAllAsync();

            // get all tags
            var tags = await tagRepository.GetAllAsync();

            var posts = blogPosts.Where(p => p.Tags.Any(t => tagids.Contains(t.Id))).ToList();

            return PartialView("_PostsList", posts);
        }
    }
}
