using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace DesignPatternChallenge
{
    public sealed class ConfigurationManager
    {
        private static readonly Lazy<ConfigurationManager> _instance =
            new Lazy<ConfigurationManager>(() => new ConfigurationManager(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static ConfigurationManager Instance => _instance.Value;

        private readonly object _lock = new object();
        private Dictionary<string, string> _settings;
        private bool _isLoaded;

        private ConfigurationManager()
        {
            _settings = new Dictionary<string, string>();
            _isLoaded = false;
        }

        private void EnsureLoaded()
        {
            if (_isLoaded) return;

            lock (_lock)
            {
                if (_isLoaded) return;

                // Simula carregamento custoso (arquivo/env/banco)
                Thread.Sleep(200);

                _settings["DatabaseConnection"] = "Server=localhost;Database=MyApp;";
                _settings["ApiKey"] = "abc123xyz789";
                _settings["CacheServer"] = "redis://localhost:6379";
                _settings["LogLevel"] = "Information";

                _isLoaded = true;
            }
        }

        public string GetSetting(string key)
        {
            EnsureLoaded();
            return _settings.TryGetValue(key, out var value) ? value : null;
        }

        // Exemplo “real”: expor visão somente leitura (evita global state bagunçado)
        public IReadOnlyDictionary<string, string> GetAllSettings()
        {
            EnsureLoaded();
            return new ReadOnlyDictionary<string, string>(_settings);
        }

        // Exemplo “real”: permitir reload controlado (útil em produção)
        public void Reload()
        {
            lock (_lock)
            {
                _isLoaded = false;
                _settings = new Dictionary<string, string>();
            }

            EnsureLoaded();
        }
    }

    public class DatabaseService
    {
        public void Connect()
        {
            var conn = ConfigurationManager.Instance.GetSetting("DatabaseConnection");
            Console.WriteLine($"[DatabaseService] {conn}");
        }
    }

    class Program
    {
        static void Main()
        {
            var db = new DatabaseService();
            db.Connect();

            Console.WriteLine(ConfigurationManager.Instance.GetSetting("LogLevel"));
        }
    }
}