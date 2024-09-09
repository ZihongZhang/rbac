using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using rbac.Modals.Models;
using rbac.Repository.Base;
using SqlSugar;

namespace rbac.Controllers
{
    public class AuthController : BaseController
    {
        private readonly ISqlSugarClient _db;

        public AuthController(ISqlSugarClient sqlSugar,Repository<User> repository)
        {
            _db = sqlSugar;
            Repository = repository;
        }

        public Repository<User> Repository { get; }

        [HttpGet]
        public async Task<ActionResult<User>> getUser()
        {
        //     return Ok(await _db.Queryable<User>().ToListAsync());
        return Ok(await Repository.GetListAsync());
        
        }
        
    }
}
