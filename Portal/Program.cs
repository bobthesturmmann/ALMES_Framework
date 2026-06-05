using _Core.Shared.Lib;
using Core.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using System.Runtime.Loader;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(AuthConstants.CookieScheme)
    .AddCookie(AuthConstants.CookieScheme, options =>
    {
        options.LoginPath = "/Auth/Account/Login";
        options.LogoutPath = "/Auth/Account/Logout";
        options.Cookie.Name = AuthConstants.CookieScheme;
        options.AccessDeniedPath = "/Auth/Account/Login";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ModuleControl", policy =>
        policy.Requirements.Add(new _Core.Shared.Lib.ModuleRequirement()));
});

var mvcBuilder = builder.Services.AddControllersWithViews();
var embeddedProviders = new List<IFileProvider>();

string currentExecutionPath = AppDomain.CurrentDomain.BaseDirectory;
string modulesRootPath = Path.Combine(currentExecutionPath, "Modules");

if (!Directory.Exists(modulesRootPath))
{
    modulesRootPath = Path.Combine(currentExecutionPath, "..", "..", "..", "..", "Server", "Modules");
}

string sharedPath = Path.Combine(modulesRootPath, "Shared");

if (!Directory.Exists(modulesRootPath))
{
    string solutionPath = Path.GetFullPath(Path.Combine(currentExecutionPath, "..", "..", "..", ".."));

    if (!Directory.Exists(Path.Combine(solutionPath, "Server", "Modules")))
    {
        solutionPath = Path.GetFullPath(Path.Combine(currentExecutionPath, "..", "..", "..", "..", ".."));
    }

    modulesRootPath = Path.Combine(solutionPath, "Server", "Modules");
}

if (Directory.Exists(modulesRootPath))
{
    var moduleDirectories = Directory.GetDirectories(modulesRootPath);

    foreach (var dir in moduleDirectories)
    {
        if (Path.GetFileName(dir).Equals("Shared", StringComparison.OrdinalIgnoreCase)) continue;

        var allDlls = Directory.GetFiles(dir, "*.dll");
        foreach (var dllPath in allDlls)
        {
            if (!dllPath.EndsWith(".UI.dll"))
            {
                try { AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath); } catch { }
            }
        }

        var uiDlls = Directory.GetFiles(dir, "*.UI.dll");
        foreach (var dllPath in uiDlls)
        {
            try
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath);
                mvcBuilder.AddApplicationPart(assembly).AddControllersAsServices();
                embeddedProviders.Add(new EmbeddedFileProvider(assembly));
            }
            catch { }
        }
    }
}

if (embeddedProviders.Any())
{
    builder.Services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
    {
        options.AreaViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
        options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
    });

    builder.Environment.ContentRootFileProvider = new CompositeFileProvider(
        new[] { builder.Environment.ContentRootFileProvider }.Concat(embeddedProviders)
    );
}

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<SqlEngine>();

builder.Services.AddScoped<IModuleConnectionProvider, ModuleConnectionProvider>();

var activeModules = builder.Configuration.GetSection("ActiveModules").Get<List<string>>() ?? new List<string>();

var hiddenModules = builder.Configuration.GetSection("HiddenModules").Get<List<string>>() ?? new List<string>();

var allTargetModules = activeModules.Concat(hiddenModules).Distinct().ToList();

var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

if (Directory.Exists(modulesRootPath))
{
    var moduleDirectories = Directory.GetDirectories(modulesRootPath);
    foreach (var dir in moduleDirectories)
    {
        if (Path.GetFileName(dir).Equals("Shared", StringComparison.OrdinalIgnoreCase)) continue;

        var allDlls = Directory.GetFiles(dir, "*.dll");
        foreach (var dllPath in allDlls)
            try
            {
                var loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath);
                if (!loadedAssemblies.Contains(loadedAssembly))
                {
                    loadedAssemblies.Add(loadedAssembly);
                }
            }
            catch { }
    }
}

foreach (var assembly in loadedAssemblies)
{
    string? assemblyName = assembly.GetName().Name;
    if (assemblyName == null) continue;

    bool isOurAssembly = assemblyName.StartsWith("Core", StringComparison.OrdinalIgnoreCase) ||
                         assemblyName.StartsWith("_Core", StringComparison.OrdinalIgnoreCase) ||
                         assemblyName.StartsWith("Bom", StringComparison.OrdinalIgnoreCase) ||
                         assemblyName.StartsWith("Auth", StringComparison.OrdinalIgnoreCase);

    if (!isOurAssembly) continue;

    try
    {
        var allTypes = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract);

        foreach (var type in allTypes)
        {
            var interfaces = type.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                if (@interface.Name == "I" + type.Name ||
                    @interface.Namespace?.Contains("Shared") == true ||
                    @interface.Name == "IAuthorizationHandler")
                {
                    builder.Services.AddScoped(@interface, type);
                }
            }
        }
    }
    catch { }

    bool isTargetModule = allTargetModules.Any(mod =>
        assemblyName.StartsWith(mod + ".", StringComparison.OrdinalIgnoreCase) ||
        assemblyName.Equals(mod, StringComparison.OrdinalIgnoreCase) ||
        assemblyName.StartsWith("_Core.", StringComparison.OrdinalIgnoreCase) ||
        assemblyName.StartsWith("Core.", StringComparison.OrdinalIgnoreCase));

    if (isTargetModule)
    {
        try
        {
            var typesToRegister = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract &&
                    (t.Name.EndsWith("Repository") ||
                     t.Name.EndsWith("Manager") ||
                     t.Name.EndsWith("Controller") ||
                     t.Name.EndsWith("Service") ||
                     t.Name.EndsWith("Bridge")));

            foreach (var type in typesToRegister)
            {
                builder.Services.AddTransient(type);
            }
        }
        catch { }
    }
}

//void CopyActiveModulesStaticFiles(IConfiguration configuration)
//{
//    string currentPath = AppDomain.CurrentDomain.BaseDirectory;

//    var activeModules = configuration.GetSection("ActiveModules").Get<List<string>>() ?? new List<string>();

//    string portalWwwRoot = Path.Combine(currentPath, "wwwroot");
//    if (!Directory.Exists(portalWwwRoot)) Directory.CreateDirectory(portalWwwRoot);

//    string solutionPath = Path.GetFullPath(Path.Combine(currentPath, "..", "..", "..", ".."));
//    string modulesRootPath = Path.Combine(solutionPath, "Modules");

//    if (!Directory.Exists(modulesRootPath))
//    {
//        solutionPath = Path.GetFullPath(Path.Combine(currentPath, "..", "..", "..", "..", ".."));
//        modulesRootPath = Path.Combine(solutionPath, "Modules");
//    }

//    foreach (var module in activeModules)
//    {
//        if (module.Equals("Core", StringComparison.OrdinalIgnoreCase)) continue;

//        string moduleWwwRoot = Path.Combine(modulesRootPath, module, $"{module}.UI", "wwwroot");

//        if (Directory.Exists(moduleWwwRoot))
//        {
//            string targetModuleFolder = Path.Combine(portalWwwRoot, module.ToLower());
//            if (!Directory.Exists(targetModuleFolder)) Directory.CreateDirectory(targetModuleFolder);

//            foreach (string dirPath in Directory.GetDirectories(moduleWwwRoot, "*", SearchOption.AllDirectories))
//            {
//                Directory.CreateDirectory(dirPath.Replace(moduleWwwRoot, targetModuleFolder));
//            }

//            foreach (string newPath in Directory.GetDirectories(moduleWwwRoot, "*", SearchOption.AllDirectories)
//                                             .SelectMany(d => Directory.GetFiles(d))
//                                             .Concat(Directory.GetFiles(moduleWwwRoot)))
//            {
//                string destFile = newPath.Replace(moduleWwwRoot, targetModuleFolder);
//                if (!File.Exists(destFile) || File.GetLastWriteTime(newPath) > File.GetLastWriteTime(destFile))
//                {
//                    File.Copy(newPath, destFile, true);
//                }
//            }
//        }
//    }
//}

//CopyActiveModulesStaticFiles(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

if (Directory.Exists(modulesRootPath))
{
    var moduleDirectories = Directory.GetDirectories(modulesRootPath);
    foreach (var dir in moduleDirectories)
    {
        if (Path.GetFileName(dir).Equals("Shared", StringComparison.OrdinalIgnoreCase)) continue;

        string moduleWwwRoot = Path.Combine(dir, "wwwroot");

        if (Directory.Exists(moduleWwwRoot))
        {
            var subDirs = Directory.GetDirectories(moduleWwwRoot);
            if (subDirs.Any())
            {
                string sourceContentFolder = subDirs.First();
                string actualFolderName = Path.GetFileName(sourceContentFolder).ToLower();

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(sourceContentFolder),

                    RequestPath = $"/{actualFolderName}",

                    OnPrepareResponse = ctx =>
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
                    }
                });
            }
        }
    }
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "bom_special",
    pattern: "Bom/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Portal}/{action=Index}/{id?}");

app.Run();