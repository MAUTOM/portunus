// Copyright (c) 2020 Bence Horváth <horvath.bence@mautom.hu>.
//
// This file is part of Portunus OpenPGP key server 
// (see https://www.horvathb.dev/portunus).
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Mautom.Portunus.Config;
using Mautom.Portunus.Contracts;
using Mautom.Portunus.Entities;
using Mautom.Portunus.Extensions;
using Mautom.Portunus.Gpg;
using Mautom.Portunus.Mail;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;

namespace Mautom.Portunus
{
    public class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            var config = ConfigManager.Configuration;

            #region Signing key password storage

            if (args.Contains("--with-root-password"))
            {
                Console.WriteLine(
                    "WARNING! It is recommended that the signing key password be provided by an EXTERNAL source (such as via pinentry).");
                Console.WriteLine("Entering PGP root key password setting mode.");

                var secure = new SecureString();
                var secure2 = new SecureString();
                ConsoleKeyInfo key;

                Console.Write("Enter PGP root key password: ");
                do
                {
                    key = Console.ReadKey(true);
                    if ((int) key.Key < 65 || (int) key.Key > 90) continue;
                    // Append the character to the password.
                    secure.AppendChar(key.KeyChar);
                    Console.Write("*");
                } while (key.Key != ConsoleKey.Enter);

                Console.WriteLine();
                Console.Write("Repeat password: ");
                do
                {
                    key = Console.ReadKey(true);
                    if ((int) key.Key < 65 || (int) key.Key > 90) continue;
                    // Append the character to the password.
                    secure2.AppendChar(key.KeyChar);
                    Console.Write("*");
                } while (key.Key != ConsoleKey.Enter);

                Console.WriteLine();

                if (!secure.Equals(secure2))
                {
                    Console.WriteLine("Passwords do not match!");
                    Environment.Exit(-1);
                }

                ConfigManager.RootPassword = secure;
                ConfigManager.RootPassword.MakeReadOnly();
                secure.Dispose();
                secure2.Dispose();

                Console.WriteLine("Signing key password set, continuing startup...");
                Thread.Sleep(1500);
            }

            #endregion

            var certificateSettings = config.GetSection("certificateSettings");
            string certificateFileName = certificateSettings.GetValue<string>("fileName");
            string certificatePassword = certificateSettings.GetValue<string>("password");

            Log.Info($"Setting certificate file to: {certificateFileName}");

            var certificate = new X509Certificate2(certificateFileName, certificatePassword);

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GNUPGHOME")))
            {
                Log.Info("GNUPGHOME not set, falling back to config file.");
                var gpgHome = ConfigManager.PgpSettings["HomeDir"];
                GpgKeychain.Instance.HomeDir = gpgHome;
                Environment.SetEnvironmentVariable("GNUPGHOME", gpgHome);
                Log.Info($"Set GNUPGHOME to {gpgHome}");
            }

            if (ConfigManager.PgpSettings.GetValue<bool>("ConfirmPurge"))
            {
                Log.Info("Purging " + GpgKeychain.Instance.HomeDir);
                Log.Info("CONFIRM WITH ENTER");
                Console.ReadLine();
            }

            GpgKeychain.Instance.Purge();

            AppDomain.CurrentDomain.ProcessExit += ExitLogic;
            Console.CancelKeyPress += ExitLogic;

            var host = new WebHostBuilder()
                .UseKestrel(
                    options =>
                    {
                        options.AddServerHeader = false;
                        options.Listen(IPAddress.Parse(ConfigManager.ApiSettings.GetValue("Host", "localhost")),
                            ConfigManager.ApiSettings.GetValue<int>("Port"),
                            listenOptions => { listenOptions.UseHttps(certificate); });
                        options.Listen(IPAddress.Loopback, 5000);
                    }
                )
                .UseConfiguration(config)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls(
                    $"https://{ConfigManager.ApiSettings.GetValue<string>("Host")}:{ConfigManager.ApiSettings.GetValue<int>("Port")}",
                    "http://localhost:5000")
                .Build();

            using var scope = host.Services.CreateScope();
            var repoManager = scope.ServiceProvider.GetRequiredService<IRepositoryManager>();

            if (ConfigManager.PgpSettings.GetValue<bool>("LoadFromDatabase"))
                GpgKeychain.Instance.ImportAllKeys(repoManager.PublicKey);

            MailManager.Instance.RunDispatcher();

            host.Run();
        }

        private static void ExitLogic(object? o, EventArgs e)
        {
            GpgKeychain.Instance.Dispose();
            MailManager.Instance.Dispose();
        }

        private static void CreateDbIfNotExists(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<RepositoryContext>();
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred creating the DB.");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}