using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using rbac.Modals.Dto;
using rbac.Modals.Models;
using rbac.Repository.Base;
using SqlSugar;

namespace rbac.Controllers
{
    public class AuthController : BaseController
    {
        private readonly ISqlSugarClient _db;
        public Repository<User> Repository { get; }

        public AuthController(ISqlSugarClient sqlSugar,Repository<User> repository)
        {
            _db = sqlSugar;
            Repository = repository;
        }


        // [HttpPost]
        // public async Task<ActionResult<string>> Login(LoginDto login)
        // {
            

            

                    
        // }
        
    }
}
