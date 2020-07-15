using AutoMapper;
using Mautom.Portunus.Contracts;
using Microsoft.AspNetCore.Mvc;

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
    }
}