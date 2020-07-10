using System;
using System.Collections.Generic;
using AutoMapper;
using MAKeys.Contracts;
using MAKeys.Entities.DataTransferObjects;
using MAKeys.Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace MAKeysd.Controllers
{
    [Route("api/publickeys")]
    [ApiController]
    public class PublicKeyController : ControllerBase
    {
        private readonly ILoggerManager _logger;
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;
        
        // GET
        public PublicKeyController(ILoggerManager logger, IRepositoryManager manager, IMapper mapper)
        {
            _logger = logger;
            _repository = manager;
            _mapper = mapper;
        }
        
        [HttpGet]
        public IActionResult GetAllPublicKeys()
        {
            try
            {
                var pubKeys = _repository.PublicKey.GetAllPublicKeys();
                _logger.LogInfo("Fetched all public keys from database");

                var pubKeyResult = _mapper.Map<IEnumerable<PublicKeyDto>>(pubKeys);

                return Ok(pubKeyResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong in GetAllPublicKeys(): {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
        
        [HttpGet("{fingerprint}", Name = "PublicKeyByFingerprint")]
        public IActionResult GetPublicKeyByFingerprint(string fingerprint)
        {
            try
            {


                var key = _repository.PublicKey.GetPublicKeyByFingerprint(fingerprint);

                if (key == null)
                {
                    _logger.LogError($"Cannot find key with fingerprint {fingerprint}");
                    return NotFound();
                }
                else
                {
                    _logger.LogInfo($"Fetched key {fingerprint}");
                    var keyResult = _mapper.Map<PublicKeyDto>(key);
                    return Ok(keyResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetPublicKeyByFingerprint: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost]
        public IActionResult CreatePublicKey([FromBody] PublicKeyForCreationDto key)
        {
            try
            {
                if (key == null)
                {
                    _logger.LogError("Public key object received from client is NULL!");
                    return BadRequest("Public key is NULL!");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid object received from client.");
                    return BadRequest("Invalid model object.");
                }

                var keyEntity = _mapper.Map<PublicKey>(key);
                _repository.PublicKey.CreatePublicKey(keyEntity);
                _repository.Save();

                var createdKey = _mapper.Map<PublicKeyDto>(keyEntity);

                return CreatedAtRoute("PublicKeyByFingerprint", new {Fingerprint = createdKey.Fingerprint}, createdKey);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside CreatePublicKey: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}