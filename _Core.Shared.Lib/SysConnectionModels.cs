using System;

namespace _Core.Shared.Lib
{
    public class SysConnectionModel
    {
        public int Id { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string FirmaNo { get; set; } = string.Empty;
        public string DonemNo { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class ResolvedModuleSettings
    {
        public string ModuleName { get; set; } = string.Empty;
        public string FirmaNo { get; set; } = string.Empty;
        public string DonemNo { get; set; } = string.Empty;
        public string ProcedurePrefix { get; set; } = string.Empty;
    }
}