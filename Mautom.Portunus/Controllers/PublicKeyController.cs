// Copyright (c) 2020 Bence Horváth <horvath.bence@mautom.hu>.
//
// This file is part of Portunus OpenPGP key server 
// (see https://www.horvathb.dev/portunus).
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using AutoMapper;
using Mautom.Portunus.Contracts;
using Mautom.Portunus.Entities.DataTransferObjects;
using Mautom.Portunus.Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace Mautom.Portunus.Controllers
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