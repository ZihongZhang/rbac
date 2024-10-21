using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using rbac.CoreBusiness.Dtos;
using rbac.CoreBusiness.Qms;
using rbac.CoreBusiness.Services;
using rbac.CoreBusiness.Vms;
using rbac.Infra.Helper;
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
        public AiService _aiService { get; }

        public AuthController(ISqlSugarClient sqlSugar,Repository<User> repository,UserServices userServices,ILogger<AuthController> logger,AiService aiService)
        {
            _db = sqlSugar;
            Repository = repository;
            _userServices = userServices;
            _logger = logger;
            _aiService = aiService;
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

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("info")]
        public async Task<ActionResult<InfoVm>> GetCurrentUserInfo()
        {
            var userInfo = await _userServices.GetInfoAsync();
            return Ok(userInfo);
        }

        

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("add-user")]
        public async Task<ActionResult> AddUser(UserDto userDto)
        {
            string result = await _userServices.AddUserAsync(userDto);
            return Ok(result);
        }

        /// <summary>
        /// 获取全部用户，不确定是否会使用
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("get-all-users")]
        public async Task<ActionResult> GetAllUsers()
        {
            var result = await _userServices.GetAllUsersAsync();
            return Ok(result);            
        }

        /// <summary>
        /// 获取该角色的菜单
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("get-all-menus")]
        public async Task<ActionResult> GetAllMenus()
        {
            var res = await _userServices.GetMenuList();
            return Ok(res);
        }

        /// <summary>
        /// 获取所有分页用户
        /// </summary>
        /// <param name="userQms"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("paged-users")]
        public async Task<ActionResult> GetPagedUsers([FromQuery]UserQms userQms)
        {
            var result =await _userServices.GetPagedUsersAsync(userQms);
            return Ok(result);           
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <param name="userQms"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("get-roles")]
        public async Task<ActionResult> GetRoleListAsyc()
        {
            var result =await _userServices.GetRoleListAsyc();
            return Ok(result);           
        }

        /// <summary>
        /// 更新用户信息以及角色
        /// </summary>
        /// <param name="userVm"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("update-user")]
        public async Task<ActionResult> UpdateUser(UserVm userVm)
        {
            var res = await _userServices.UpdateUserAsync(userVm);
            return Ok(res);
        }
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="userVm"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("delete-user")]
        public async Task<ActionResult> DeleteUser(UserVm userVm)
        {
            var res = await _userServices.DeleteUserAsync(userVm);
            return Ok(res);
        }


        /// <summary>
        /// 更新角色对应的菜单
        /// </summary>
        /// <param name="roleVm"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("update-role-menu")]
        public async Task<ActionResult> updateRoleMenu(RoleVm roleVm)
        {
            var res = await _userServices.UpdateRolesMenuAsync(roleVm);
            return Ok(res);
        }
        
        /// <summary>
        /// 增加新的角色
        /// </summary>
        /// <param name="roleVm"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("add-role")]
        public async Task<ActionResult> AddRoleAsync(RoleVm roleVm)
        {
            var res = await _userServices.AddRoleAsync(roleVm);
            return Ok(res);
        }
        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="roleVm"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("delete-role")]
        public async Task<ActionResult> DeleteRoleAsync(RoleVm roleVm)
        {
            var res = await _userServices.DeleteRoleAsync(roleVm);
            return Ok(res);
        }

         
    }
}
