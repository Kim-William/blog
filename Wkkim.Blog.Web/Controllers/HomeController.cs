using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Net;

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
        private readonly IConfiguration configuration;
        private readonly string email_id;
        private readonly string email_password;

        public HomeController(ILogger<HomeController> logger,
            IBlogPostRepository blogPostRepository,
            ITagRepository tagRepository,
            IConfiguration configuration)
        {
            _logger = logger;
            this.blogPostRepository = blogPostRepository;
            this.tagRepository = tagRepository;
            this.configuration = configuration;
            email_id = configuration.GetSection("EMail")["ID"];
            email_password = configuration.GetSection("EMail")["Password"];
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

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Contact")]
        public async Task<IActionResult> Contact(ContactInfo query)
        {
            SendEmail(query);
            return View();
        }

        public void SendEmail(ContactInfo query)
        {
            // 이메일 설정
            try
            {
                string smtpAddress = "smtp.gmail.com";
                int portNumber = 587;
                bool enableSSL = true;
                string subject = $"New Contact Request from Wkkim Blog";
                string body = $"Thank you for contacting me.<br>I will personally review this email and respond shortly.<br>Below are the name, email, and message of the person who requested to contact me.<br>Name: {query.Name}<br>Email: {query.EMail}<br>Message:{query.Message}";

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(email_id);
                    mail.To.Add(email_id);
                    mail.To.Add(query.EMail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(email_id, email_password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                }
                ViewBag.Message = "Thank you! Email sent successfully!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
            }
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
