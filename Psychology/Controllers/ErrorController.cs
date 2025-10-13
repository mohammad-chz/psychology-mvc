using Microsoft.AspNetCore.Mvc;

namespace Psychology.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/500")]
        public IActionResult Error500(string? traceId)
        {
            ViewBag.TraceId = traceId;
            return View("Error500");
        }

        [Route("Error/{code:int}")]
        public IActionResult ErrorCode(int code)
        {
            Response.StatusCode = code;
            return View($"Error{code}");
        }
    }
}
