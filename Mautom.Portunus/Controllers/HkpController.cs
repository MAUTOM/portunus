using System;
using System.Linq;
using System.Net.Mime;
using AutoMapper;
using Mautom.Portunus.Contracts;
using Mautom.Portunus.Entities.DataTransferObjects;
using Mautom.Portunus.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

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
        public IActionResult Lookup([FromQuery] string op, [FromQuery] string search, [FromQuery] string? options)
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
                        else if (search.Length == 16)
                        {
                            key = _repository.PublicKey.GetAllPublicKeys(false)
                                .FirstOrDefault(k => k.Fingerprint.LongKeyId.Equals(search, StringComparison.InvariantCultureIgnoreCase));
                        }

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

                    return StatusCode(501, "Not Implemented");


                default:
                    return BadRequest();
            }
        }
    }
}