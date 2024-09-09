using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using rbac.Modals.Models;
using SqlSugar;

namespace rbac.Controllers
{
    public class AuthController : BaseController
    {
        private readonly ISqlSugarClient _db;

        public AuthController(ISqlSugarClient sqlSugar)
        {
            _db = sqlSugar;
        }
        [HttpGet]
        public async Task<ActionResult<User>> getUser()
        {
            return Ok(await _db.Queryable<User>().ToListAsync());

        }
        
    }
}
