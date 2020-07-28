using System.Net.Mime;
using AutoMapper;
using Mautom.Portunus.Contracts;
using Mautom.Portunus.Shared.Pgp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
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
       
    }
}