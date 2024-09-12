using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using rbac.Infra;
using rbac.Infra.Exceptions;
using rbac.Infra.FunctionalInterfaces;
using rbac.Infra.Helper;
using rbac.Modals.Dto;
using rbac.Modals.Models;
using rbac.Repository.Base;

namespace rbac.CoreBusiness.Services;

public class UserServices : IScoped
{
    public Repository<User> _userRepository { get; }

    public UserServices(Repository<User> repository)
    {
        _userRepository = repository;
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="loginDto"></param>
    /// <returns></returns>
    public async Task<string> LoginAsync(LoginDto loginDto)
    {
        CheckHelper.NotNull(loginDto);
        User user = await _userRepository.GetSingleAsync(a => a.Username == loginDto.UserName);
        if (user == null) throw new DomainException("不存在该用户");
        if(string.Equals(user.Password,loginDto.Password))
        {
            return GenerateToken(user);
        }
        throw new DomainException("登录失败");
    }

    /// <summary>
    /// 产生token
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public string GenerateToken( User user)
    {
        var Claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email,user.Email),
            new Claim(ClaimTypes.Name,user.Username),
            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString())
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
