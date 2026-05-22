using Core.Service;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

var mvcBuilder = builder.Services.AddControllersWithViews();
var embeddedProviders = new List<IFileProvider>();

string currentExecutionPath = AppDomain.CurrentDomain.BaseDirectory;
string modulesRootPath = Path.Combine(currentExecutionPath, "MODULES");

if (!Directory.Exists(modulesRootPath))
{
    modulesRootPath = Path.Combine(currentExecutionPath, "..", "..", "..", "..", "Server", "MODULES");
}

if (Directory.Exists(modulesRootPath))
{
    var moduleDirectories = Directory.GetDirectories(modulesRootPath);

    foreach (var dir in moduleDirectories)
    {
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

builder.Services.AddSingleton<SqlEngine>();

var activeModules = builder.Configuration.GetSection("ActiveModules").Get<List<string>>() ?? new List<string>();
var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

if (Directory.Exists(modulesRootPath))
{
    var moduleDirectories = Directory.GetDirectories(modulesRootPath);
    foreach (var dir in moduleDirectories)
    {
        var allDlls = Directory.GetFiles(dir, "*.dll");
        foreach (var dllPath in allDlls)
        {
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
}

foreach (var assembly in loadedAssemblies)
{
    string? assemblyName = assembly.GetName().Name;
    if (assemblyName != null)
    {
        bool isModuleActive = activeModules.Any(mod => assemblyName.StartsWith(mod + ".", StringComparison.OrdinalIgnoreCase)
                                                     || assemblyName.Equals(mod, StringComparison.OrdinalIgnoreCase));

        if (isModuleActive)
        {
            var typesToRegister = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && (t.Name.EndsWith("Repository") || t.Name.EndsWith("Manager")));

            foreach (var type in typesToRegister)
            {
                builder.Services.AddTransient(type);
            }
        }
    }
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=BOM}/{controller=BomHome}/{action=Index}/{id?}");

app.Run();