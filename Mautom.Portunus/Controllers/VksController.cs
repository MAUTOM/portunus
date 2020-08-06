using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AutoMapper;
using Libgpgme;
using Mautom.Portunus.Config;
using Mautom.Portunus.Contracts;
using Mautom.Portunus.Entities.DataTransferObjects;
using Mautom.Portunus.Entities.Models;
using Mautom.Portunus.Gpg;
using Mautom.Portunus.Mail;
using Mautom.Portunus.Shared.Pgp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using MimeKit;
using MimeKit.Cryptography;
using MimeKit.Text;
using Newtonsoft.Json;
using NLog;
using ContentDisposition = System.Net.Mime.ContentDisposition;
using ContentType = System.Net.Mime.ContentType;
using PublicKeyAlgorithm = Mautom.Portunus.Shared.Pgp.PublicKeyAlgorithm;

namespace Mautom.Portunus.Controllers
{
    [ApiController]
    [Route("vks/v1")]
    public class VksController : ControllerBase
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;
        
        public VksController(IRepositoryManager manager, IMapper mapper)
        {
            _repository = manager;
            _mapper = mapper; 
        }
        
        /// <summary>
        /// Retrieves the key with the given Fingerprint.
        /// </summary>
        /// <param name="fingerprint">The Fingerprint refers to the primary key. Hexadecimal digits MUST be uppercase, and MUST NOT be prefixed with 0x.</param>
        /// <returns>The returned key is ASCII Armored, and has a content-type of application/pgp-keys. </returns>
        [HttpGet("by-fingerprint/{fingerprint}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetByFingerprint(string fingerprint)
        {
            if (!PublicKeyFingerprint.IsValidFingerprint(fingerprint))
            {
                Log.Debug($"Got bad fingerprint request: {fingerprint}");
                return BadRequest();
            }

            var key = _repository.PublicKey.GetPublicKeyByFingerprint(fingerprint, false);

            if (key == null)
            {
                Log.Debug($"Got request for non-existent fingerprint: {fingerprint}");
                return NotFound();
            }

            Log.Info($"Fetched key with fingerprint: [{fingerprint}]");
            
            Response.Headers[HeaderNames.ContentDisposition] = new ContentDisposition
            {
                FileName = $"{key.Fingerprint}.asc"
            }.ToString();
            
            return Content(key.ArmoredKey, "application/pgp-keys");
        }

        /// <summary>
        /// Retrieves the key with the given long KeyID.
        /// </summary>
        /// <param name="longKeyId">The KeyID refers to the primary key. Hexadecimal digits MUST be uppercase, and MUST NOT be prefixed with 0x.</param>
        /// <returns>The returned key is ASCII Armored, and has a content-type of application/pgp-keys. </returns>
        [HttpGet("by-keyid/{longKeyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetByKeyId(string longKeyId)
        {
            var key = _repository.PublicKey.GetPublicKeyByKeyId(longKeyId, false);
            
            if (key == null)
            {
                Log.Debug($"Got request for non-existent key ID: {longKeyId}");
                return NotFound();
            }

            Log.Info($"Fetched key with key ID: [{longKeyId}]");
            
            Response.Headers[HeaderNames.ContentDisposition] = new ContentDisposition
            {
                FileName = $"{key.Fingerprint}.asc"
            }.ToString();

            return Content(key.ArmoredKey, "application/pgp-keys");
        }

        /// <summary>
        /// Retrieves the key with the given Email Address.
        /// Only exact matches are accepted.
        /// Lookup by email address requires opt-in by the owner of the email address. T
        /// </summary>
        /// <param name="email">Only exact matches are accepted.</param>
        /// <returns>he returned key is ASCII Armored, and has a content-type of application/pgp-keys. </returns>
        [HttpGet("by-email/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetByEmail(string email)
        {
            var identity = _repository.KeyIdentity.GetIdentityByEmail(email, false);

            if (identity == null)
            {
                Log.Debug($"Identity with e-mail <{email}> does not exist in DB.");
                return NotFound();
            }

            var key = identity.PublicKey;
            
            Log.Info($"Fetched key with e-mail: [{email}]");
            
            Response.Headers[HeaderNames.ContentDisposition] = new ContentDisposition
            {
                FileName = $"{key.Fingerprint}.asc"
            }.ToString();

            return Content(key.ArmoredKey, "application/pgp-keys");
        }

        [HttpPost("upload")]
        public IActionResult Upload(KeyUploadParameters up)
        {
            if (string.IsNullOrEmpty(up.KeyText))
                return BadRequest();

            var result = GpgKeychain.Instance.ImportArmoredKey(up.KeyText);

            foreach (var importResult in result.Imports)
            {
                var key = (PgpKey) GpgKeychain.Instance.KeyStore.GetKey(importResult.Fpr, false);

                if (_repository.PublicKey.GetPublicKeyByFingerprint(importResult.Fpr, false) != null)
                {
                    Log.Warn($"Submission of already existing Fpr: {key.Fingerprint}. Skipping.");
                    continue;
                }
                
                Log.Info(
                    $"Imported fpr:{key.Fingerprint}, created at: {key.Uid.Signatures.Timestamp}, identities: {key.Uids.Count()}");


                var armor = GpgKeychain.Instance.ExportArmoredKey(key.Fingerprint);

                // create new db record

                // create identities
                var verificationToken = Guid.NewGuid();
                
                var identities = key.Uids.Select(uid => new KeyIdentity
                {
                    Name = uid.Name, Email = uid.Email, Comment = uid.Comment, CreationDate = uid.Signatures.Timestamp,
                    Status = IdentityStatus.Unpublished, VerificationToken = verificationToken
                }).ToList();

                var statusList = identities.ToDictionary(k => k.Email, val => val.Status);

                // create key record
                var keyFingerprint = new PublicKeyFingerprint(key.Fingerprint);
                var flags = PublicKeyFlags.Default;
                if (key.Disabled) flags |= PublicKeyFlags.Disabled;
                if (key.Expired) flags |= PublicKeyFlags.Expired;
                if (key.Revoked) flags |= PublicKeyFlags.Revoked;

                var publicKey = new PublicKey
                {
                    Fingerprint = keyFingerprint,
                    LongKeyId = keyFingerprint.LongKeyId,
                    ShortKeyId = keyFingerprint.ShortKeyId,
                    CreationDate = key.Subkeys.Timestamp,
                    Algorithm = (PublicKeyAlgorithm) key.Subkeys.PubkeyAlgorithm,
                    Length = (PublicKeyLength) key.Subkeys.Length,
                    ArmoredKey = armor,
                    Flags = flags,
                    KeyIdentities = identities
                };

                _repository.PublicKey.CreatePublicKey(publicKey);
                _repository.Save();
                
                key.Dispose();
                
                // produce response
                var response = new KeyUploadResult(key.Fingerprint, verificationToken, statusList);

                return Ok(response);
            }
            
            return Ok();
        }

        [HttpPost("request-verify")]
        public IActionResult RequestVerify(RequestVerifyParameters vp)
        {
            Log.Info(JsonConvert.SerializeObject(vp));

            var identities = _repository.KeyIdentity.GetIdentitiesByToken(vp.Token).ToList();

            if (identities.Count == 0)
                return NotFound();

            if (vp.Addresses.Any(address => !identities.Any(id => id.Email.Equals(address, StringComparison.OrdinalIgnoreCase))))
            {
                return BadRequest("One of the specified addresses does not exist.");
            }

            var affectedIdentities = identities.Where(id => vp.Addresses.Contains(id.Email) && id.Status == IdentityStatus.Unpublished).ToList();

            // all specified addresses can be verified.
            
            var rnd = new Random();
            var statusList = new Dictionary<string, IdentityStatus>();
            foreach (var identity in affectedIdentities)
            {
                var verification = new AddressVerification
                {
                    Token = vp.Token,
                    Email = identity.Email,
                    VerificationCode = (ushort) rnd.Next(10000, ushort.MaxValue)
                };
                
                _repository.AddressVerification.Create(verification);
                statusList.Add(identity.Email, IdentityStatus.Pending);
                identity.Status = IdentityStatus.Pending;
                _repository.KeyIdentity.Update(identity);
                // TODO: send mail
                
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("MAUTOM Portunus", ConfigManager.SmtpSettings.GetValue<string>("Sender")));
                message.To.Add(new MailboxAddress(identity.Name, identity.Email));
                message.Subject = "Verify your identity";
                
                var msgBody = $@"Dear {identity.Name},
You are receiving this e-mail because you have requested the verification of your PGP key and e-mail address.
Your e-mail address is: {identity.Email}
Please click the link below, to verify your address and publish it on the server:

{CreateVerificationUrl(vp.Token, verification.VerificationCode)}

Please do not reply to this e-mail.";

                var emailBody = GpgKeychain.Instance.EncryptAndSign(msgBody, identity.PublicKeyFingerprint);

                message.Body = new TextPart(TextFormat.Plain) { Text = emailBody };
                
                MailManager.Instance.Enqueue(message);
            }
            
            
            _repository.Save();

            var response = new KeyUploadResult(identities[0].PublicKeyFingerprint, vp.Token, statusList);
            
            return Ok(response);
        }
        
        [HttpGet("verify/{token}", Name = "Verify")]
        public IActionResult Verify(Guid token, [FromQuery] ushort secret)
        {
            if (secret < 10000)
                return BadRequest();
            
            Log.Debug($"Verify request for token {token}, secret: {secret}.");

            var verifications = _repository.AddressVerification.GetAddressVerificationsByToken(token).ToList();

            foreach (var verification in verifications)
            {
                if (verification.VerificationCode != secret) continue;
                var identity = _repository.KeyIdentity.GetIdentityByEmail(verification.Email);
                    
                if (identity == null)
                {
                    Log.Error("No matching identity found for address verification.");
                    return StatusCode(500, "Internal Server Error");
                }
                    
                identity.Status = IdentityStatus.Published;
                _repository.KeyIdentity.Update(identity);

                identity.PublicKey.Flags |= PublicKeyFlags.Verified;
                _repository.PublicKey.Update(identity.PublicKey);
                    
                _repository.AddressVerification.Remove(verification);

                Log.Info($"Verified address: {identity.Email}");
            }
            
            _repository.Save();

            return Content("E-mail address successfully validated.", "text/plain");
        }


        [HttpGet("test")]
        public IActionResult Test()
        {
            return Content(
                GpgKeychain.Instance.EncryptAndSign("hello there", "33EFA0592FAEEF4DD84CD8A0E4C22D9F57CBD3F0"), "text/plain");
        }

        /// <summary>
        /// Creates a VKS verification URL for the specified token and secret.
        /// </summary>
        /// <param name="verificationToken">The verification GUID received upon key submission</param>
        /// <param name="verificationSecret">The secret (5 digits)</param>
        /// <returns>The verification URL</returns>
        private string CreateVerificationUrl(Guid verificationToken, ushort verificationSecret)
        {
            return new Uri(ConfigManager.BaseUrl, Url.Action("Verify", "Vks", new {token = verificationToken, secret = verificationSecret})).ToString();
        }
    }
}