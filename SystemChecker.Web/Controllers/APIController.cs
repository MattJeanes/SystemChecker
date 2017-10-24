using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SystemChecker.Model.Data;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Enums;
using SystemChecker.Model.Data.Interfaces;
using SystemChecker.Model.DTO;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace SystemChecker.Web.Controllers
{
    [Route("api")]
    public class APIController : Controller
    {
        private readonly ICheckerUow _uow;
        private readonly IMapper _mapper;
        public APIController(ICheckerUow uow, IMapper mapper) {
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<List<CheckDTO>> GetAll()
        {
            var checks = await _uow.Checks.GetAll().ToListAsync();
            return _mapper.Map<List<CheckDTO>>(checks);
        }

        [HttpGet("{id:int}")]
        public async Task<CheckDetailDTO> GetDetails(int id)
        {
            var check = await _uow.Checks.GetDetails(id);
            return _mapper.Map<CheckDetailDTO>(check);
        }

        [HttpGet("types")]
        public async Task<List<CheckTypeDTO>> GetTypes()
        {
            var types = await _uow.CheckTypes.GetAll().ToListAsync();
            return _mapper.Map<List<CheckTypeDTO>>(types);
        }

        [HttpGet("settings")]
        public async Task<Settings> GetSettings()
        {
            var logins = await _uow.Logins.GetAll().ToListAsync();
            var connStrings = await _uow.ConnStrings.GetAll().ToListAsync();
            return new Settings {
                Logins = _mapper.Map<List<LoginDTO>>(logins),
                ConnStrings = _mapper.Map<List<ConnStringDTO>>(connStrings)
            };
        }

        [HttpPost]
        public CheckDetailDTO Edit([FromBody]CheckDetailDTO value)
        {
            return value;
        }

        [HttpPost("settings")]
        public Settings SetSettings([FromBody]Settings value)
        {
            return value;
        }

        [HttpPost("run/{id:int}")]
        public bool StartRun(int id)
        {
            return true;
        }

        [HttpDelete("{id:int}")]
        public bool Delete(int id)
        {
            return true;
        }
    }
}
