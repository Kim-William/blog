using System.Net;

using Microsoft.AspNetCore.Mvc.Filters;

namespace Wkkim.Blog.Web.Data
{
    public class LogIpAddressFilter : IActionFilter
    {
        private readonly ILogger<LogIpAddressFilter> _logger;

        public LogIpAddressFilter(ILogger<LogIpAddressFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var hostname = Dns.GetHostName();
            var ipList = Dns.GetHostEntry(hostname).AddressList;

            var ipAddress = string.Empty;

            if (ipList!= null && ipList.Count()>0)
            {
                ipAddress = string.Join("|", ipList.Select(t => t.ToString()));
            }

            _logger.LogInformation($"Request from [{ipAddress}]");
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do nothing after the action executes
        }
    }
}
