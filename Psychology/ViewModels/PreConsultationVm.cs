using Microsoft.AspNetCore.Mvc.Rendering;
using Psychology.Validation;
using System.ComponentModel.DataAnnotations;

namespace Psychology.ViewModels
{
    [ValidPhoneByPrefix] // اعتبارسنجی سطح کلاس برای بررسی هماهنگی پیش‌شماره با شماره
    public class PreConsultationVm
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "انتخاب پیش‌شماره الزامی است.")]
        [Display(Name = "پیش‌شماره کشور")]
        public int PhonePrefixId { get; set; }

        [Required(ErrorMessage = "وارد کردن شماره تماس الزامی است.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "شماره تماس باید فقط شامل اعداد باشد.")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "شماره تماس باید بین ۵ تا ۲۰ رقم باشد.")]
        [Display(Name = "شماره تماس")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "وارد کردن ایمیل الزامی است.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل وارد شده صحیح نیست.")]
        [MaxLength(100, ErrorMessage = "طول ایمیل نمی‌تواند بیش از ۱۰۰ کاراکتر باشد.")]
        [Display(Name = "آدرس ایمیل")]
        public string Email { get; set; } = string.Empty;

        public List<SelectListItem> Prefixes { get; set; } = new();
    }
}
