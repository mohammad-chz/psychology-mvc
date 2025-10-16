using Microsoft.EntityFrameworkCore;
using Psychology.Domain.Entities;
using Psychology.Infrastructure.Persistence;
using Psychology.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace Psychology.Validation
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ValidPhoneByPrefixAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is not PreConsultationVm vm) return ValidationResult.Success;

            var db = (AppDbContext)context.GetService(typeof(AppDbContext))!;
            var prefix = db.Set<PhonePrefix>().AsNoTracking()
                           .FirstOrDefault(p => p.Id == vm.PhonePrefixId && p.IsActive);

            if (prefix is null)
                return new ValidationResult("پیش‌شماره نامعتبر است.");

            var digits = new string((vm.PhoneNumber ?? "").Where(char.IsDigit).ToArray());
            if (digits.Length != prefix.ExpectedLength)
                return new ValidationResult($"برای {prefix.CountryName}، شماره باید دقیقاً {prefix.ExpectedLength} رقم باشد.");

            return ValidationResult.Success;
        }
    }
}
