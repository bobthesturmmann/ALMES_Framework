using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace _Core.Shared.Lib
{
    public class AppSettingsService : IAppSettingsService
    {
        private readonly IConfiguration _configuration;
        private readonly string _jsonPath;

        public AppSettingsService(IConfiguration configuration)
        {
            _configuration = configuration;
            _jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            if (!File.Exists(_jsonPath))
            {
                _jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            }
        }

        public List<string> GetActiveModules()
        {
            return _configuration.GetSection("ActiveModules").Get<List<string>>() ?? new List<string>();
        }

        public GlobalAlmesSettings GetGlobalSettings()
        {
            return new GlobalAlmesSettings
            {
                SirketKodu = _configuration["AlmesSettings:SirketKodu"] ?? "000",
                DonemKodu = _configuration["AlmesSettings:DonemKodu"] ?? "00",
                GlobalConnectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty
            };
        }

        public void UpdateGlobalSettings(string sirketKodu, string donemKodu, string globalConnectionString)
        {
            if (!File.Exists(_jsonPath)) return;

            try
            {
                var jsonString = File.ReadAllText(_jsonPath);
                var jObject = JsonNode.Parse(jsonString)?.AsObject();

                if (jObject != null)
                {
                    if (jObject["AlmesSettings"] == null) jObject["AlmesSettings"] = new JsonObject();
                    jObject["AlmesSettings"]!["SirketKodu"] = sirketKodu.Trim();
                    jObject["AlmesSettings"]!["DonemKodu"] = donemKodu.Trim();

                    if (jObject["ConnectionStrings"] == null) jObject["ConnectionStrings"] = new JsonObject();
                    jObject["ConnectionStrings"]!["DefaultConnection"] = globalConnectionString.Trim();

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string updatedJson = jObject.ToJsonString(options);
                    File.WriteAllText(_jsonPath, updatedJson);

                    _configuration["AlmesSettings:SirketKodu"] = sirketKodu.Trim();
                    _configuration["AlmesSettings:DonemKodu"] = donemKodu.Trim();
                    _configuration["ConnectionStrings:DefaultConnection"] = globalConnectionString.Trim();
                }
            }
            catch { }
        }
    }
}