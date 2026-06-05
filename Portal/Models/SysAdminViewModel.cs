using System.Collections.Generic;
using _Core.Shared.Lib;

namespace Portal.Models
{
    public class SysAdminViewModel
    {
        public string GlobalSirketKodu { get; set; } = string.Empty;
        public string GlobalDonemKodu { get; set; } = string.Empty;
        public string GlobalConnectionString { get; set; } = string.Empty;

        public List<string> ActiveModules { get; set; } = new List<string>();

        public List<string> ShownModules { get; set; } = new List<string>();

        public List<string> SelectedShownModules { get; set; } = new List<string>();

        public List<SysConnectionModel> SavedConnections { get; set; } = new List<SysConnectionModel>();

        public SysConnectionModel CurrentModel { get; set; } = new SysConnectionModel();
    }
}