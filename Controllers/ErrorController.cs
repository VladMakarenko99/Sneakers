using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;

namespace practice.Controllers
{
    [Route("[controller]")]
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            var exceptionHandler = HttpContext.Features.Get<IExceptionHandlerFeature>();
            ViewBag.exceptionPath = exceptionHandler?.Path;
            ViewBag.exceptionMessage = exceptionHandler?.Error.Message;
            
            return View();
        }
    }
}