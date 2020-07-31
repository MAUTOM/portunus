using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using AutoMapper;
using Libgpgme;
using Mautom.Portunus.Contracts;
using Mautom.Portunus.Entities.DataTransferObjects;
using Mautom.Portunus.Entities.Models;
using Mautom.Portunus.Shared.Pgp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using NLog;

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
            
            //Log.Debug($"Got keytext: {up.KeyText}");
            using var gpgContext = new Context {KeylistMode = KeylistMode.Signatures, Armor = true};
            var store = gpgContext.KeyStore;
            var encoder = new UTF8Encoding();
            var keyData = encoder.GetBytes(up.KeyText);

            var mStream = new MemoryStream(keyData);
            var gpgStream = new GpgmeStreamData(mStream);

            var result = store.Import(gpgStream);

            foreach (var importResult in result.Imports)
            {
                var key = (PgpKey) store.GetKey(importResult.Fpr, false);

                if (_repository.PublicKey.GetPublicKeyByFingerprint(importResult.Fpr, false) != null)
                {
                    Log.Warn($"Submission of already existing Fpr: {key.Fingerprint}. Skipping.");
                    continue;
                }
                
                Log.Info(
                    $"Imported fpr:{key.Fingerprint}, created at: {key.Uid.Signatures.Timestamp}, identities: {key.Uids.Count()}");

                var data = new GpgmeMemoryData {Encoding = DataEncoding.Armor};
                store.Export(key.Fingerprint, data);
                data.Position = 0;

                using var reader = new StreamReader(data, Encoding.ASCII);
                var armor = reader.ReadToEnd();

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
                var flags = PublicKeyFlags.None;
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
                
                store.DeleteKey(key, false);

                key.Dispose();
                
                // produce response
                var response = new KeyUploadResult(key.Fingerprint, verificationToken, statusList);

                return Ok(response);

                //Console.WriteLine($"Read gpg data: {data.Length}\nArmor: {armor}");
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
            
            // all specified addresses can be verified.
            
            var rnd = new Random();

            foreach (var address in vp.Addresses)
            {
                var verification = new AddressVerification
                {
                    Token = vp.Token,
                    Email = address,
                    VerificationCode = (ushort) rnd.Next(10000, ushort.MaxValue)
                };
                
                _repository.AddressVerification.Create(verification);
            }
            
            _repository.Save();
            
            return Ok();
        }
       
    }
}