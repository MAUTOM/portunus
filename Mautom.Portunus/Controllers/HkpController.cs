using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using AutoMapper;
using Libgpgme;
using Mautom.Portunus.Config;
using Mautom.Portunus.Contracts;
using Mautom.Portunus.Entities.DataTransferObjects;
using Mautom.Portunus.Entities.Models;
using Mautom.Portunus.Formatters;
using Mautom.Portunus.Gpg;
using Mautom.Portunus.Shared.Pgp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using MoreLinq.Extensions;
using NLog;

namespace Mautom.Portunus.Controllers
{
    [Route("pks")]
    [ApiController]
    public class HkpController : ControllerBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger(); 
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;

        public HkpController(IRepositoryManager manager, IMapper mapper)
        {
            _repository = manager;
            _mapper = mapper;
        }

        [Route("/pks/lookup")]
        [HttpGet(Name = "LookupHkp")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult Lookup([FromQuery] string op, [FromQuery] string search,
            [FromQuery] string? options,
            [FromQuery] bool? exact)
        {
            _logger.Debug(
                $"[REQUEST: /pks/lookup/]: {{op={op}, search={search}, options={options}, exact={exact}}}");

            op = op.ToLower();
            options ??= "mr";
            exact ??= false;


            switch (op)
            {
                case "get":
                    if (search.StartsWith("0x"))
                    {
                        search = search.Trim('\n', '\r');
                        search = search.Remove(0, 2);
                        _logger.Info($"Searching for {search}");

                        if (search.Length != 40 && search.Length != 16 && search.Length != 8)
                            return BadRequest();

                        var key = _repository.PublicKey.GetPublicKeyByKeyId(search, ConfigManager.SupplyOnlyPublishedKeys, false);

                        if (key == null)
                            return NotFound();

                        if (!options.Contains("mr"))
                        {
                            return Content(key.ArmoredKey, "text/plain");
                        }

                        Response.Headers[HeaderNames.ContentDisposition] = new ContentDisposition
                        {
                            FileName = $"{key.Fingerprint}.asc"
                        }.ToString();

                        var resp = Content(key.ArmoredKey, contentType: "application/pgp-keys");

                        return resp;
                    }
                    else
                    {
                        // do string search here
                        var possibleIdentities =
                            _repository.KeyIdentity.SearchIdentities(search, (bool) exact, false).ToList();

                        if (possibleIdentities.Count > 0)
                        {
                            if (!options.Contains("mr"))
                            {
                                var resultHuman = Content(possibleIdentities.First().PublicKey.ArmoredKey,
                                    "text/plain");

                                return resultHuman;
                            }
                            
                            Response.Headers[HeaderNames.ContentDisposition] = new ContentDisposition
                            {
                                FileName = $"{possibleIdentities.First().PublicKey.Fingerprint.ToString()}.asc"
                            }.ToString();
                            
                            var resultMachine = Content(possibleIdentities.First().PublicKey.ArmoredKey,
                                "application/pgp-keys");


                            return resultMachine;
                        }

                        return NotFound();
                    }

                case "index":
                    if (search.StartsWith("0x"))
                    {
                        search = search.Trim('\n', '\r');
                        search = search.Remove(0, 2);
                        _logger.Info($"Searching for {search}");

                        if (search.Length != 40 && search.Length != 16 && search.Length != 8)
                            return StatusCode(501, "Not Implemented");

                        // should be only one key with any ID

                        var key = _repository.PublicKey.GetPublicKeyByKeyId(search, ConfigManager.SupplyOnlyPublishedKeys, false);

                        var pubKeyResult = _mapper.Map<PublicKeyDto>(key);

                        if (options.Contains("mr"))
                        {
                            var result = new StringBuilder();
                            result.AppendLine("info:1:1");

                            HkpOutputFormatter.FormatHkpPublicKey(result, pubKeyResult);

                            return Content(result.ToString(), "text/plain");
                        }

                        return StatusCode(501, "Not Implemented");
                    }
                    else
                    {
                        // do string search here
                        var possibleIdentities =
                            _repository.KeyIdentity.SearchIdentities(search, (bool) exact, false).ToList();

                        if (possibleIdentities.Count == 0)
                            return NotFound();

                        var result = new StringBuilder();

                        var withUniqueFingerprints =
                            possibleIdentities.DistinctBy(id => id.PublicKeyFingerprint).ToList();

                        result.AppendLine($"info:1:{withUniqueFingerprints.Count}");

                        foreach (var identity in withUniqueFingerprints)
                        {
                            var keyDto = _mapper.Map<PublicKeyDto>(identity.PublicKey);
                            HkpOutputFormatter.FormatHkpPublicKey(result, keyDto);
                        }

                        return Content(result.ToString(), "text/plain");
                    }

                default:
                    return BadRequest();
            }
        }

        [Route("/pks/add")]
        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SubmitKey([FromForm] string keytext)
        {
            if (string.IsNullOrEmpty(keytext))
                return BadRequest();

            _logger.Info("Received HKP key submission");

            var result = GpgKeychain.Instance.ImportArmoredKey(keytext);

            foreach (var importResult in result.Imports)
            {
                var key = (PgpKey) GpgKeychain.Instance.KeyStore.GetKey(importResult.Fpr, false);
                
                if (_repository.PublicKey.GetPublicKeyByFingerprint(importResult.Fpr, false, false) != null)
                {
                    _logger.Warn($"Submission of already existing Fpr: {key.Fingerprint}. Skipping.");
                    continue;
                }
                
                _logger.Info(
                    $"Imported fpr:{key.Fingerprint}, created at: {key.Uid.Signatures.Timestamp}, identities: {key.Uids.Count()}");


                var armor = GpgKeychain.Instance.ExportArmoredKey(key.Fingerprint);

                // create new db record

                // create identities
                var identities = key.Uids.Select(uid => new KeyIdentity
                {
                    Name = uid.Name, Email = uid.Email, Comment = uid.Comment, CreationDate = uid.Signatures.Timestamp
                }).ToList();

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

                key.Dispose();

                //Console.WriteLine($"Read gpg data: {data.Length}\nArmor: {armor}");
            }

            _repository.Save();

            return Ok();
        }
        
#if DEBUG
        [Route("/pks/testgpg")]
        [HttpGet]
        public IActionResult TestGpg()
        {
            using var context = new Context();
            var store = context.KeyStore;

            var keys = store.GetKeyList("*", false);

            Console.WriteLine("GPG keys:");
            foreach (var key in keys)
            {
                Console.WriteLine($"{key.Fingerprint}");
            }

            return Ok();
        }
#endif        
    }
}