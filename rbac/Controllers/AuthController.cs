using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using rbac.CoreBusiness.Services;
using rbac.Infra.Helper;
using rbac.Modals.Dto;
using rbac.Modals.Models;
using rbac.Repository.Base;
using SqlSugar;

namespace rbac.Controllers
{
    public class AuthController : BaseController
    {
        private readonly ISqlSugarClient _db;
        private readonly UserServices _userServices;
        private readonly ILogger<AuthController> _logger;

        public Repository<User> Repository { get; }

        public AuthController(ISqlSugarClient sqlSugar,Repository<User> repository,UserServices userServices,ILogger<AuthController> logger)
        {
            _db = sqlSugar;
            Repository = repository;
            _userServices = userServices;
            _logger = logger;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<string>> Login(LoginDto login)
        {
            string token = await _userServices.LoginAsync(login);
            return Ok(token);                    
        }
        
    }
}
