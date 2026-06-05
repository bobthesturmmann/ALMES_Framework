namespace _Core.Shared.Lib
{
    public interface IAppSettingsService
    {
        List<string> GetActiveModules();
        GlobalAlmesSettings GetGlobalSettings();
        void UpdateGlobalSettings(string sirketKodu, string donemKodu, string globalConnectionString);
    }

    public class GlobalAlmesSettings
    {
        public string SirketKodu { get; set; } = "000";
        public string DonemKodu { get; set; } = "00";
        public string GlobalConnectionString { get; set; } = string.Empty;
    }
}