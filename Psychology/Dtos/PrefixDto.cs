using Microsoft.AspNetCore.Mvc.Rendering;
using Psychology.ViewModels;

namespace Psychology.Dtos
{
    public class PrefixDto
    {
        //public PreConsultationVm PreConsult { get; set; } = new();
        //public List<SelectListItem> PrefixItems { get; set; } = new();
        public Dictionary<int, PrefixMetaVm> PrefixMetaById { get; set; } = new();
    }
}
