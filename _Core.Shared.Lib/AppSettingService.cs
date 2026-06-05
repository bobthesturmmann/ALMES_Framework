using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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

        private string ConvertToMd5(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input.ToUpper().Trim());
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public List<string> GetActiveModules()
        {
            return _configuration.GetSection("ActiveModules").Get<List<string>>() ?? new List<string>();
        }

        public List<string> GetHiddenModules()
        {
            return _configuration.GetSection("HiddenModules").Get<List<string>>() ?? new List<string>();
        }

        public List<string> GetShownModules()
        {
            var hashedList = _configuration.GetSection("ShownModules").Get<List<string>>() ?? new List<string>();
            var activeModules = GetActiveModules() ?? new List<string>();
            var resolvedShownModules = new List<string>();

            if (activeModules.Count == 0)
            {
                return resolvedShownModules;
            }

            if (hashedList.Count == 0)
            {
                foreach (var module in activeModules)
                {
                    if (string.IsNullOrEmpty(module)) continue;
                    resolvedShownModules.Add(module.ToUpper().Trim());
                }

                UpdateShownModules(activeModules);
                return resolvedShownModules;
            }

            foreach (var module in activeModules)
            {
                if (string.IsNullOrEmpty(module)) continue;

                string normalHash = ConvertToMd5(module);
                string upperHash = ConvertToMd5(module.ToUpper());

                if (hashedList.Contains(normalHash) || hashedList.Contains(upperHash))
                {
                    resolvedShownModules.Add(module.ToUpper().Trim());
                }
            }

            return resolvedShownModules;
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

        public void UpdateShownModules(List<string> shownModules)
        {
            if (!File.Exists(_jsonPath)) return;

            try
            {
                var jsonString = File.ReadAllText(_jsonPath);
                var jObject = JsonNode.Parse(jsonString)?.AsObject();

                if (jObject != null)
                {
                    var hashedList = shownModules
                        .Where(m => !string.IsNullOrEmpty(m))
                        .Select(m => ConvertToMd5(m))
                        .ToList();

                    jObject.Remove("ShownModules");

                    var jsonArray = new JsonArray();
                    foreach (var hash in hashedList)
                    {
                        jsonArray.Add(hash);
                    }
                    jObject.Add("ShownModules", jsonArray);

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string updatedJson = jObject.ToJsonString(options);
                    File.WriteAllText(_jsonPath, updatedJson);

                    var section = _configuration.GetSection("ShownModules");

                    for (int i = 0; i < 20; i++)
                    {
                        if (_configuration[$"ShownModules:{i}"] == null) break;
                        _configuration[$"ShownModules:{i}"] = null;
                    }

                    for (int i = 0; i < hashedList.Count; i++)
                    {
                        _configuration[$"ShownModules:{i}"] = hashedList[i];
                    }
                }
            }
            catch { }
        }
    }
}