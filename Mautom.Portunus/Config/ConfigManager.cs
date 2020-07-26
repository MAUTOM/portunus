using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NLog;

namespace Mautom.Portunus.Config
{
    public static class ConfigManager
    {
        public static IConfigurationRoot Configuration { get; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
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
            Logger.Debug($"Configuration providers: {Configuration.Providers.ToList()}");
        }
    }
}