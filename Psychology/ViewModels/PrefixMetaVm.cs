namespace Psychology.ViewModels
{
    public sealed class PrefixMetaVm
    {
        public int Id { get; set; }
        public string CountryName { get; set; } = "";
        public string Prefix { get; set; } = "";        // like “+98”
        public int ExpectedLength { get; set; }          // digits for the local part
    }
}
