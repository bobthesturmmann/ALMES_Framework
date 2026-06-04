namespace _Core.Shared.Lib
{
    public interface IModuleConnectionProvider
    {
        string GetConnectionString(string moduleName, string firmaNo, string donemNo);

        ResolvedModuleSettings ResolveModuleSettings(string moduleName);
    }
}