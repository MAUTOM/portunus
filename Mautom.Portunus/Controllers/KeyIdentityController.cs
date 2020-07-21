using System.Collections.Generic;
using AutoMapper;
using Mautom.Portunus.Contracts;
using Mautom.Portunus.Entities.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

namespace Mautom.Portunus.Controllers
{
    [Route("api/publickeys/{publicKeyFingerprint}/identities")]
    [ApiController]
    public class KeyIdentityController : ControllerBase
    {
        private readonly ILoggerManager _logger;
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;
        
        public KeyIdentityController(ILoggerManager logger, IRepositoryManager manager, IMapper mapper)
        {
            _logger = logger;
            _repository = manager;
            _mapper = mapper;
        }
    
        [HttpGet]
        public IActionResult GetIdentitiesForPublicKey(string publicKeyFingerprint)
        {
            var key = _repository.PublicKey.GetPublicKeyByFingerprint(publicKeyFingerprint, trackChanges: false);

            if (key == null)
            {
                _logger.LogInfo($"Key with fingerprint {publicKeyFingerprint} not found in database.");
                return NotFound();
            }

            var idDto = _mapper.Map<IEnumerable<KeyIdentityDto>>(key.KeyIdentities);

            return Ok(idDto);
        }

        [HttpGet("{email}")]
        public IActionResult GetIdentityByEmail(string publicKeyFingerprint, string email)
        {
            var key = _repository.PublicKey.GetPublicKeyByFingerprint(publicKeyFingerprint, trackChanges: false);

            if (key == null)
            {
                _logger.LogInfo($"Key with fingerprint {publicKeyFingerprint} not found in database.");
                return NotFound();
            }

            var identity = _repository.KeyIdentity.GetIdentityByEmail(publicKeyFingerprint, email, trackChanges: false);
            if (identity == null)
            {
                _logger.LogInfo($"Identity with email {email} does not exist for key {publicKeyFingerprint} in database.");
                return NotFound();
            }

            var idDto = _mapper.Map<KeyIdentityDto>(identity);

            return Ok(idDto);
        }
    }
}