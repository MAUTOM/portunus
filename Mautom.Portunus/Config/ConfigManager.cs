using System;
using System.IO;
using System.Linq;
using System.Security;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Fluent;

namespace Mautom.Portunus.Config
{
    public static class ConfigManager
    {
        public static IConfigurationRoot Configuration { get; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        public static SecureString? RootPassword { get; set; }

        public static IConfigurationSection ApiSettings => Configuration.GetSection("API");
        public static IConfigurationSection SmtpSettings => Configuration.GetSection("SMTP");
        public static IConfigurationSection LoggingSettings => Configuration.GetSection("Logging");
        public static IConfigurationSection PgpSettings => Configuration.GetSection("PGP");
        
        public static readonly Uri BaseUrl;
        
        static ConfigManager()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("Config/certificates.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"Config/certificates.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                .AddJsonFile("Config/appsettings.json", false, true)
                .AddJsonFile($"Config/appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
                .Build();

            
            Logger.Info("Initialized configuration root");

            foreach (var cfg in Configuration.Providers)
            {
                Logger.Debug($"Loaded: {cfg}");
            }
            
            BaseUrl = new Uri($"https://{ApiSettings.GetValue<string>("Host")}:{ApiSettings.GetValue<int>("Port")}", UriKind.Absolute);
            
        }
    }
}