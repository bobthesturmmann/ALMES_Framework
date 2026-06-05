using System.Collections.Generic;

namespace _Core.Shared.Lib
{
    public interface IAppSettingsService
    {
        List<string> GetActiveModules();
        List<string> GetHiddenModules();

        List<string> GetShownModules();

        GlobalAlmesSettings GetGlobalSettings();
        void UpdateGlobalSettings(string sirketKodu, string donemKodu, string globalConnectionString);

        void UpdateShownModules(List<string> shownModules);
    }

    public class GlobalAlmesSettings
    {
        public string SirketKodu { get; set; } = "000";
        public string DonemKodu { get; set; } = "00";
        public string GlobalConnectionString { get; set; } = string.Empty;
    }
}