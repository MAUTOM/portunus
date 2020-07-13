using System;
using System.Collections.Generic;
using AutoMapper;
using Mautom.Portunus.Contracts;
using Mautom.Portunus.Entities.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

namespace Mautom.Portunus.Controllers
{
    [Route("api/identities")]
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
        public IActionResult GetAllIdentities()
        {
            try
            {
                var identities = _repository.KeyIdentity.GetAllKeyIdentities();
                _logger.LogInfo("Fetched all key identities");
                var identityResult = _mapper.Map<IEnumerable<KeyIdentityDto>>(identities);

                return Ok(identityResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something wrong in GetAllIdentities(): {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}