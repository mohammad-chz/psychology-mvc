using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Psychology.Application.Interfaces;
using Psychology.Domain.Entities;
using Psychology.Infrastructure.Persistence;
using Psychology.ViewModels;

namespace Psychology.Controllers
{
    public class PreConsultationController(IUnitOfWork uow) : Controller
    {
        // Call this from your page action (Home/Index or wherever you show the modal)
        public async Task<(PreConsultationVm vm, string json)> BuildVmAsync(CancellationToken ct = default)
        {
            var prefixes = await uow.PhonePrefixes
                 .GetListAsync(
                 predicate: p => p.IsActive,
                 orderBy: p => p.OrderBy(p => p.CountryName),
                 include: null,
                 selector: p => new
                 {
                     p.Id,
                     p.CountryName,
                     p.Prefix,
                     p.ExpectedLength,
                 },
                 ct: ct
                 );

            var vm = new PreConsultationVm
            {
                Prefixes = prefixes.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.CountryName} ({p.Prefix})"
                }).ToList()
            };

            var map = prefixes.ToDictionary(
                p => p.Id.ToString(),
                p => new { prefix = p.Prefix, len = p.ExpectedLength, name = p.CountryName }
            );

            var json = System.Text.Json.JsonSerializer.Serialize(map);
            return (vm, json);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] PreConsultationVm vm, CancellationToken ct)
        {
            var prefixes = await uow.PhonePrefixes
                .GetListAsync(
                predicate: p => p.IsActive,
                orderBy: p => p.OrderBy(p => p.CountryName),
                include: null,
                selector: p => new
                {
                    p.Id,
                    p.CountryName,
                    p.Prefix,
                    p.ExpectedLength,
                },
                ct: ct
                );

            vm.Prefixes = prefixes.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.CountryName} ({p.Prefix})"
            }).ToList();

            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    ok = false,
                    errors = ModelState
                    .Where(kv => kv.Value!.Errors.Any())
                    .ToDictionary(kv => kv.Key, kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray())
                });

            // TODO: save your entity:
            // var entity = new PreConsultation {
            //   PhonePrefixId = vm.PhonePrefixId,
            //   PhoneNumber   = new string(vm.PhoneNumber.Where(char.IsDigit).ToArray())
            // };
            // _db.Add(entity);
            // await _db.SaveChangesAsync(ct);

            return Ok(new { ok = true, message = "درخواست شما ثبت شد." });
        }
    }
}
