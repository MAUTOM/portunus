using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Libgpgme;
using Mautom.Portunus.Config;
using Mautom.Portunus.Contracts;
using Mautom.Portunus.Shared.Pgp;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Fluent;

namespace Mautom.Portunus.Gpg
{
    public sealed class GpgKeychain : IDisposable
    {
        #region Singleton
        
        private static GpgKeychain _instance = null!;
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        public static GpgKeychain Instance => _instance ??= new GpgKeychain();
        
        #endregion

        private readonly Context _context;
        private readonly UTF8Encoding _encoding;

        public IKeyStore KeyStore => _context.KeyStore;
        public string SigningKeyFingerprint { get; }
        
        private GpgKeychain()
        {
            _context = new Context {KeylistMode = KeylistMode.Signatures, Armor = true};

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GNUPGHOME")))
                _context.EngineInfo.HomeDir = Environment.GetEnvironmentVariable("GNUPGHOME");
            
            _encoding = new UTF8Encoding();
            _log.Info($"Initialized GPG keychain with homedir: {_context.EngineInfo.HomeDir}");
            
            var config = ConfigManager.Configuration;
            var pgp = config.GetSection("PGP");
            SigningKeyFingerprint = pgp.GetValue<string>("SigningKey");
        }

        /// <summary>
        /// Imports the GPG keys contained within the specified armored key text.
        /// </summary>
        /// <param name="armoredKey">Armored PGP keyring</param>
        /// <returns>The enumerable import results.</returns>
        public ImportResult ImportArmoredKey(string armoredKey)
        {
            var data = _encoding.GetBytes(armoredKey);

            using var mStream = new MemoryStream(data);
            using var gpgStream = new GpgmeStreamData(mStream);
            return KeyStore.Import(gpgStream);
        }

        /// <summary>
        /// Exports the ASCII-armored keyring of the key with the specified fingerprint.
        /// </summary>
        /// <param name="fingerprint">V4 PGP fingerprint</param>
        /// <returns></returns>
        public string ExportArmoredKey(string fingerprint)
        {
            var data = new GpgmeMemoryData {Encoding = DataEncoding.Armor};
            KeyStore.Export(fingerprint, data);
            data.Position = 0;
            
            using var reader = new StreamReader(data, Encoding.ASCII);
            return reader.ReadToEnd();
        }

        public void ImportAllKeys(IPublicKeyRepository repository)
        {
            _log.Info($"Importing all PGP keys from database into GPG keyring located at: {_context.EngineInfo.HomeDir}");
            
            foreach (var pubKey in repository.GetAllPublicKeys(false))
            {
                _log.Debug($"Importing key fpr:{pubKey.Fingerprint.ToString("FP")}");
                ImportArmoredKey(pubKey.ArmoredKey);
            }
        }

        public string SignAndEncrypt(string message, params string[] fingerprints)
        {
            var targetKeys = KeyStore.GetKeyList(fingerprints, false);
            
            if(!targetKeys.Any())
                throw new PgpException("Recipient keys not found in system GPG keyring.");

            var signedMessage = ClearSignWithRootKey(message);

            return EncryptFor(signedMessage, fingerprints);
        }

        public string EncryptAndSign(string message, params string[] fingerprints)
        {
            var rootKey = (PgpKey) KeyStore.GetKey(SigningKeyFingerprint, true);
            if (rootKey == null) throw new PgpException("Specified signing key is not found in the keyring.");
            
            var targetKeys = KeyStore.GetKeyList(fingerprints, false);
            
            if(!targetKeys.Any())
                throw new PgpException("Recipient keys not found in system GPG keyring.");
            
            var plain = new GpgmeMemoryData {FileName = "temp-encrypt.txt"};
            var cipher = new GpgmeMemoryData { FileName = "temp-es-cipher.txt" };
            

            using var writer = new BinaryWriter(plain, _encoding);
            writer.Write(message.ToCharArray());
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            _context.Signers.Clear();
            _context.Signers.Add(rootKey);
            
            if(!_context.HasPassphraseFunction)
                _context.SetPassphraseFunction(GpgPasswordCallback);

            _context.EncryptAndSign(targetKeys, EncryptFlags.AlwaysTrust, plain, cipher);
            cipher.Seek(0, SeekOrigin.Begin);
            
            using var reader = new StreamReader(cipher, _encoding);
            var cipherText = reader.ReadToEnd();
            
            cipher.Dispose();
            plain.Dispose();

            return cipherText;

        }

        public string EncryptFor(string message, params string[] fingerprints)
        {
            var targetKeys = KeyStore.GetKeyList(fingerprints, false);
            
            if(!targetKeys.Any())
                throw new PgpException("Recipient keys not found in system GPG keyring.");

            var plain = new GpgmeMemoryData {FileName = "temp-encrypt.txt"};

            using var writer = new BinaryWriter(plain, _encoding);
            writer.Write(message.ToCharArray());
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);

            var cipher = new GpgmeMemoryData {FileName = "temp-cipher.txt"};

            _context.Encrypt(targetKeys, EncryptFlags.AlwaysTrust, plain, cipher);
            cipher.Seek(0, SeekOrigin.Begin);
            
            using var reader = new StreamReader(cipher, _encoding);
            var cipherText = reader.ReadToEnd();
            
            cipher.Dispose();
            plain.Dispose();

            return cipherText;

        }

        public string ClearSignWithRootKey(string message)
        {
            var rootKey = (PgpKey) KeyStore.GetKey(SigningKeyFingerprint, true);
            if (rootKey == null) throw new PgpException("Specified signing key is not found in the keyring.");

            var plain = new GpgmeMemoryData {FileName = "temp-to-sign.txt"};
            var signed = new GpgmeMemoryData {FileName = "temp-signed.txt"};

            using var writer = new BinaryWriter(plain, _encoding);
            writer.Write(message.ToCharArray());
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            
            _context.Signers.Clear();
            _context.Signers.Add(rootKey);
            
            if(!_context.HasPassphraseFunction)
                _context.SetPassphraseFunction(GpgPasswordCallback);

            var result = _context.Sign(plain, signed, SignatureMode.Clear);
            signed.Position = 0;
            using var reader = new StreamReader(signed, Encoding.UTF8);

            var signedMessage = reader.ReadToEnd();
            
            signed.Dispose();
            plain.Dispose();

            return signedMessage;
        }

        private PassphraseResult GpgPasswordCallback(Context ctx, PassphraseInfo info, ref char[] passphrase)
        {
            // TODO: implement private key password reading securely, if pinentry is not working
            // TODO: [NOTE]: This WILL NOT be called when the system has a functional pinentry setup.
            // TODO: this is for development purposes, '.rootkey-password' must be created manually.
            
            var pwFile = Path.Combine(Environment.CurrentDirectory, ".rootkey-password");

            if (!File.Exists(pwFile))
                return PassphraseResult.Canceled;

            var pw = File.ReadAllText(pwFile);
            if (string.IsNullOrEmpty(pw))
                return PassphraseResult.Canceled;

            passphrase = pw.ToCharArray();
            return PassphraseResult.Success;
        }

        public void Purge()
        {
            _log.Info($"Removing all keys in GPG keyring located at: {_context.EngineInfo.HomeDir}");
            foreach (var key in KeyStore.GetKeyList("", false))
            {
                // skip signing key if set, as that is imported manually.
                if (SigningKeyFingerprint.Equals(key.Fingerprint, StringComparison.OrdinalIgnoreCase))
                {

                    _log.Info($"Skipping {key.Fingerprint}...");
                    continue;
                }

                KeyStore.DeleteKey(key, true);
            }
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}