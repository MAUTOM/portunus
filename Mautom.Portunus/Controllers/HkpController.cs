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
using Mautom.Portunus.Formatters;
using Mautom.Portunus.Shared.Pgp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using MoreLinq.Extensions;

namespace Mautom.Portunus.Controllers
{
    [Route("pks")]
    [ApiController]
    public class HkpController : ControllerBase
    {
        private readonly ILoggerManager _logger;
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;

        public HkpController(ILoggerManager logger, IRepositoryManager manager, IMapper mapper)
        {
            _logger = logger;
            _repository = manager;
            _mapper = mapper;
        }

        [Route("/pks/lookup")]
        [HttpGet(Name = "LookupHkp")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult Lookup([FromQuery] string op, [FromQuery] string search,
            [FromQuery] string? options = "mr",
            [FromQuery] bool? exact = false)
        {
            _logger.LogDebug(
                $"[REQUEST: /pks/lookup/]: {{op={op}, search={search}, options={options}, exact={exact}}}");

            op = op.ToLowerInvariant();
            options ??= string.Empty;
            exact ??= false;


            switch (op)
            {
                case "get":
                    if (search.StartsWith("0x"))
                    {
                        search = search.Trim('\n', '\r');
                        search = search.Remove(0, 2);
                        _logger.LogInfo($"Searching for {search}");

                        if (search.Length != 40 && search.Length != 16 && search.Length != 8)
                            return BadRequest();

                        var key = _repository.PublicKey.GetPublicKeyByKeyId(search, false);

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
                        _logger.LogInfo($"Searching for {search}");

                        if (search.Length != 40 && search.Length != 16 && search.Length != 8)
                            return StatusCode(501, "Not Implemented");

                        // should be only one key with any ID

                        var key = _repository.PublicKey.GetPublicKeyByKeyId(search, false);

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

            _logger.LogInfo("Received HKP key submission");
            using var gpgContext = new Context {KeylistMode = KeylistMode.Signatures, Armor = true};

            var store = gpgContext.KeyStore;
            var encoder = new UTF8Encoding();
            var keyData = encoder.GetBytes(keytext);

            var mStream = new MemoryStream(keyData);
            var gpgStream = new GpgmeStreamData(mStream);

            var result = store.Import(gpgStream);

            foreach (var importResult in result.Imports)
            {
                var key = (PgpKey) store.GetKey(importResult.Fpr, false);

                _logger.LogInfo(
                    $"Imported fpr:{key.Fingerprint}, created at: {key.Uid.Signatures.Timestamp}, identities: {key.Uids.Count()}");

                var data = new GpgmeMemoryData {Encoding = DataEncoding.Armor};
                store.Export(key.Fingerprint, data);
                data.Position = 0;

                using var reader = new StreamReader(data, Encoding.ASCII);
                var armor = reader.ReadToEnd();

                // create new db record

                // create identities
                var identities = key.Uids.Select(uid => new KeyIdentity
                {
                    Name = uid.Name, Email = uid.Email, Comment = uid.Comment, CreationDate = uid.Signatures.Timestamp
                }).ToList();

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

                store.DeleteKey(key, false);

                key.Dispose();

                //Console.WriteLine($"Read gpg data: {data.Length}\nArmor: {armor}");
            }

            _repository.Save();

            return Ok();
        }

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
    }
}