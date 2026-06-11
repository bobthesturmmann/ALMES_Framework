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

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ModuleControl", policy =>
        policy.Requirements.Add(new _Core.Shared.Lib.ModuleRequirement()));

var mvcBuilder = builder.Services.AddControllersWithViews();
List<IFileProvider> embeddedProviders = [];

string currentExecutionPath = AppDomain.CurrentDomain.BaseDirectory;
string modulesRootPath = Path.Combine(currentExecutionPath, "Modules");

if (!Directory.Exists(modulesRootPath))
{
    modulesRootPath = Path.Combine(currentExecutionPath, "..", "..", "..", "..", "Server", "Modules");
}

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

        var allTargetDlls = Directory.GetFiles(dir, "*.dll");
        foreach (var dllPath in allTargetDlls)
        {
            if (!dllPath.EndsWith(".UI.dll", StringComparison.OrdinalIgnoreCase))
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

if (embeddedProviders.Count > 0)
{
    builder.Services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
    {
        options.AreaViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
        options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
    });

    builder.Environment.ContentRootFileProvider = new CompositeFileProvider(
        [builder.Environment.ContentRootFileProvider, .. embeddedProviders]
    );
}

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<SqlEngine>();
builder.Services.AddScoped<IModuleConnectionProvider, ModuleConnectionProvider>();

var activeModules = builder.Configuration.GetSection("ActiveModules").Get<List<string>>() ?? [];
var hiddenModules = builder.Configuration.GetSection("HiddenModules").Get<List<string>>() ?? [];

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
                    @interface.Namespace?.Contains("Shared", StringComparison.OrdinalIgnoreCase) == true ||
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
                    (t.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase) ||
                     t.Name.EndsWith("Manager", StringComparison.OrdinalIgnoreCase) ||
                     t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) ||
                     t.Name.EndsWith("Service", StringComparison.OrdinalIgnoreCase) ||
                     t.Name.EndsWith("Bridge", StringComparison.OrdinalIgnoreCase)));

            foreach (var type in typesToRegister)
            {
                builder.Services.AddTransient(type);
            }
        }
        catch { }
    }
}

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
            if (subDirs.Length > 0)
            {
                string sourceContentFolder = subDirs.First();
                string actualFolderName = Path.GetFileName(sourceContentFolder).ToLower();

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(sourceContentFolder),
                    RequestPath = $"/{actualFolderName}",
                    OnPrepareResponse = ctx =>
                    {
                        ctx.Context.Response.Headers.CacheControl = "public,max-age=31536000";
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
    name: "default",
    pattern: "{controller=Portal}/{action=Index}/{id?}");

app.Run();