using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using rbac.Infra;
using rbac.Infra.Exceptions;
using rbac.Infra.FunctionalInterfaces;
using rbac.Infra.Helper;
using rbac.Modals.Dto;
using rbac.Modals.Models;
using rbac.Repository.Base;
using SqlSugar;

namespace rbac.CoreBusiness.Services;

public class UserServices : IScoped
{
    public Repository<User> _userRepository { get; }
    public ISqlSugarClient _db { get; }
    public IHttpContextAccessor _httpContextAccessor { get; }

    public UserServices(Repository<User> repository, ISqlSugarClient db, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = repository;
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="loginDto"></param>
    /// <returns></returns>
    /// <exception cref="DomainException"></exception>
    public async Task<string> LoginAsync(LoginDto loginDto)
    {
        CheckHelper.NotNull(loginDto);
        User user = await _userRepository.GetSingleAsync(a => a.Username == loginDto.UserName);
        if (user == null) throw new DomainException("不存在该用户");
        if (string.Equals(user.Password, loginDto.Password))
        {
            return GenerateToken(user);
        }
        throw new DomainException("登录失败");
    }

    /// <summary>
    /// 添加新用户
    /// </summary>
    /// <param name="userDto"></param>
    /// <returns></returns>
    public async Task<string> AddUserAsync(UserDto userDto)
    {
        var user = userDto.Adapt<User>();

        //查看是否有重复用户名或者不存在对应的租户
        var username = _userRepository.GetFirst(a => a.Username == user.Username);
        if(username != null) throw new DomainException("用户id已存在,请更换用户名");
        var tenantId = _db.Queryable<Tenant>().Where(i => i.Id == user.TenantId).Count();
        if(tenantId==0) throw new DomainException("租户id不存在");

        // var TenantId = _httpContextAccessor.HttpContext?.User.FindFirstValue("tenantId");
        //获取当前userId并作为创建id赋值
        var UserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CheckHelper.NotNull(UserId, "token中不包含userId");
        user.CreateUserId = UserId ?? "1";
        user.Id = Guid.NewGuid().ToString();

        //使用sqlSugar导航属性来插入数据，默认新增加的用户的role是普通员工
        user.RoleList.Add(new Role
        {
            Id = "2"
        });
        await _db.InsertNav(user)
        .Include(x => x.RoleList, new InsertNavOptions()
        {
            ManyToManyNoDeleteMap = true
        })
        .ExecuteCommandAsync();
        return $"恭喜成功插入，{user.Username}";
    }

    /// <summary>
    /// 获取全部用户信息
    /// </summary>
    /// <returns></returns>
    public async Task<List<UserDto>> GetAllUser()
    {
        var user = await _userRepository.GetListAsync();
        var userDto = user.Adapt<List<UserDto>>();
        return userDto;
    }

    /// <summary>
    /// 产生token
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public string GenerateToken(User user)
    {
        const string ClaimTypeTenantId = "tenantId";
        var Claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email,user.Email),
            new Claim(ClaimTypes.Name,user.Username),
            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
            new Claim(ClaimTypeTenantId, user.TenantId?.ToString()??"0")
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSetting.GetValue("JWTSettings:TokenKey")));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var tokenOptions = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: Claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }
}
