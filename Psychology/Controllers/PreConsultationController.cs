using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Psychology.Application.Common.Interfaces;              // IEmailSender (your abstraction)
using Psychology.Application.Emails.PreConsultation;        // PreConsultationEmailModel
using Psychology.Application.Interfaces;                    // IUnitOfWork
using Psychology.Domain.Entities;
using Psychology.Helpers;
using Psychology.Infrastructure.Email;                      // ITemplateRenderer, EmailOptions
using Psychology.ViewModels;
using System.Text.Json;

namespace Psychology.Controllers
{
    public class PreConsultationController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly ITemplateRenderer _renderer;
        private readonly IEmailSender _email;
        private readonly EmailOptions _emailOptions;

        public PreConsultationController(
            IUnitOfWork uow,
            ITemplateRenderer renderer,
            IEmailSender email,
            IOptions<EmailOptions> opts // or IOptionsSnapshot<EmailOptions>
        )
        {
            _uow = uow;
            _renderer = renderer;
            _email = email;
            _emailOptions = opts.Value;
        }

        // Helper to build the VM + JSON for your page (NOT an action)
        [NonAction]
        public async Task<(PreConsultationVm vm, string json)> BuildVmAsync(CancellationToken ct = default)
        {
            var prefixes = await _uow.PhonePrefixes.GetListAsync(
                predicate: p => p.IsActive,
                orderBy: q => q.OrderBy(p => p.CountryName),
                selector: p => new
                {
                    p.Id,
                    p.CountryName,
                    p.Prefix,
                    p.ExpectedLength
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

            var json = JsonSerializer.Serialize(map);
            return (vm, json);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] PreConsultationVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    ok = false,
                    errors = ModelState
                        .Where(kv => kv.Value!.Errors.Any())
                        .ToDictionary(kv => kv.Key, kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray())
                });
            }

            var isEmailExist = await _uow.PreConsultation.AnyAsync(
                predicate: p => (p.Email == vm.Email && p.PhoneNumber == vm.PhoneNumber)
                );

            if (isEmailExist)
                return BadRequest("ایمیل و تلفن همراه قبلاً پیش مشاوره گرفته است.");

            var entity = new PreConsultation
            {
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
            };

            await _uow.PreConsultation.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            // (Optional) resolve country prefix from DB by vm.PhonePrefixId
            string countryPrefix = "+98";
            if (vm.PhonePrefixId != 0)
            {
                var prefixEntity = await _uow.PhonePrefixes.GetByIdAsync(vm.PhonePrefixId, ct);
                if (prefixEntity is not null)
                    countryPrefix = $"+{prefixEntity.Prefix}";
            }

            var model = new PreConsultationEmailModel
            {
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                CountryPrefix = countryPrefix,
                Date = entity.CreateDate ?? string.Empty,
                Time = TimeFormatHelper.InsertColonAfterTwoDigits(entity.CreateTime),
            };

            var userHtml = await _renderer.RenderAsync(
                "~/Infrastructure/Email/Templates/PreConsultation/User.cshtml", model);

            var adminHtml = await _renderer.RenderAsync(
                "~/Infrastructure/Email/Templates/PreConsultation/Admin.cshtml", model);

            await _email.SendAsync(vm.Email, "رزرو پیش‌مشاوره رایگان | کلینیک سنس", userHtml, ct);

            foreach (var admin in _emailOptions.AdminRecipients ?? Array.Empty<string>())
            {
                await _email.SendAsync(admin, "🔔 درخواست جدید پیش‌مشاوره", adminHtml, ct);
            }

            return Ok(new
            {
                ok = true,
                message = "درخواست شما با موفقیت ثبت شد. کارشناسان ما به‌زودی با شما تماس خواهند گرفت."
            });
        }
    }
}
