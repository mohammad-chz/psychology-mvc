using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Psychology.Application.Interfaces;
using Psychology.Dtos;
using Psychology.ViewModels;

namespace Psychology.Controllers
{
    public class HomeController(IUnitOfWork uow) : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Prefix(int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var prefixes = await uow.PhonePrefixes.GetListAsync(
                 predicate: p => p.IsActive,
                 orderBy: q => q.OrderBy(p => p.CountryName),
                 selector: p => new PrefixMetaVm
                 {
                     Id = p.Id,
                     CountryName = p.CountryName,
                     Prefix = p.Prefix,
                     ExpectedLength = p.ExpectedLength
                 },
                 ct: ct
             );

            var model = new PrefixDto
            {
              
                PrefixMetaById = prefixes.ToDictionary(p => p.Id, p => p)
            };

            return Ok(model);
        }
    }
}
