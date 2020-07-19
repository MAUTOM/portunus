using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using AutoMapper;
using Mautom.Portunus.Contracts;
using Mautom.Portunus.Entities.DataTransferObjects;
using Mautom.Portunus.Entities.Models;
using Mautom.Portunus.Formatters;
using Mautom.Portunus.Shared;
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
        public IActionResult Lookup([FromQuery] string op, [FromQuery] string search, [FromQuery] string options = "mr",
            [FromQuery] bool exact = false)
        {
            op = op.ToLowerInvariant();
            options ??= string.Empty;

            switch (op)
            {
                case "get":
                    if (search.StartsWith("0x"))
                    {
                        search = search.Trim('\n', '\r');
                        search = search.Remove(0, 2);
                        _logger.LogInfo($"Searching for {search}");
                        PublicKey? key = null;
                        if (search.Length == 40)
                        {
                            key = _repository.PublicKey.GetPublicKeyByFingerprint(search, false);
                        }
                        else if (search.Length == 16 || search.Length == 8)
                        {
                            // TODO: maybe add short and long key IDs to a separate DB field upon key insertion, so that it is queryable here
                            // TODO cont: EF Core cannot setup a query for a runtime computed value, so we fetch everything and use LINQ
                            var keys = _repository.PublicKey.GetAllPublicKeys(trackChanges: false);

                            key = keys.FirstOrDefault(k =>
                                k.Fingerprint.LongKeyId.Equals(search, StringComparison.InvariantCultureIgnoreCase) ||
                                k.Fingerprint.ShortKeyId.Equals(search, StringComparison.InvariantCultureIgnoreCase));
                        }
                        else
                            return StatusCode(501, "Not Implemented");

                        if (key == null)
                            return NotFound();

                        if (!options.Contains("mr"))
                            return Content(key.ArmoredKey, "text/plain");

                        Response.Headers[HeaderNames.ContentDisposition] = new ContentDisposition
                        {
                            FileName = $"{key.Fingerprint}.asc"
                        }.ToString();

                        return Content(key.ArmoredKey, contentType: "application/pgp-keys");
                    }
                    else
                    {
                        // do string search here
                        var possibleIdentities =
                            _repository.KeyIdentity.SearchIdentities(search, exact, false).ToList();

                        if (possibleIdentities.Count > 0)
                        {
                            if (!options.Contains("mr"))
                                return Content(possibleIdentities.First().PublicKey.ArmoredKey, "text/plain");


                            return Content(possibleIdentities.First().PublicKey.ArmoredKey, "application/pgp-keys");
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
                        
                        // TODO: optimize: don't get all keys here
                        
                        var keys = _repository.PublicKey.GetAllPublicKeys(false)
                            .Where(pk =>
                                pk.Fingerprint.Fingerprint.Equals(search, StringComparison.InvariantCultureIgnoreCase)
                                || pk.Fingerprint.LongKeyId.Equals(search, StringComparison.InvariantCultureIgnoreCase)
                                || pk.Fingerprint.ShortKeyId.Equals(search,
                                    StringComparison.InvariantCultureIgnoreCase));

                        var pubKeyResult = _mapper.Map<IEnumerable<PublicKeyDto>>(keys).ToList();

                        if (options.Contains("mr"))
                        {
                            var result = new StringBuilder();
                            result.AppendLine($"info:1:{pubKeyResult.Count}");

                            foreach (var key in pubKeyResult)
                                HkpOutputFormatter.FormatHkpPublicKey(result, key);

                            return Content(result.ToString(), "text/plain");
                        }

                        return StatusCode(501, "Not Implemented");
                    }
                    else
                    {
                        // do string search here
                        var possibleIdentities =
                            _repository.KeyIdentity.SearchIdentities(search, exact, false).ToList();

                        if (possibleIdentities.Count == 0)
                            return NotFound();
                        
                        var result = new StringBuilder();

                        var withUniqueFingerprints = possibleIdentities.DistinctBy(id => id.PublicKeyFingerprint).ToList();
                        
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
    }
}