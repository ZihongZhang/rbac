using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Mapster;
using MapsterMapper;
using Masuit.Tools;
using Masuit.Tools.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using rbac.CoreBusiness.Dtos;
using rbac.CoreBusiness.Qms;
using rbac.CoreBusiness.Vms;
using rbac.Infra;
using rbac.Infra.Exceptions;
using rbac.Infra.FunctionalInterfaces;
using rbac.Infra.Helper;
using rbac.Modals.Enum;
using rbac.Modals.Models;
using rbac.Repository.Base;
using SqlSugar;

namespace rbac.CoreBusiness.Services;

public class UserServices : IScoped
{
   // private readonly IMapper _mapper;

    public Repository<User> _userRepository { get; }
    public ISqlSugarClient _db { get; }
    public IHttpContextAccessor _httpContextAccessor { get; }

    public UserServices(Repository<User> repository, ISqlSugarClient db, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = repository;
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        //_mapper = mapper;
    }

    #region 登录，添加新用户，获取用户,获取菜单信息

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
        if (user.Status == StatusEnum.Disable) throw new DomainException("该用户已被禁用");
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
        if(string.IsNullOrWhiteSpace(userDto.Username)||string.IsNullOrWhiteSpace(userDto.Password)) throw new DomainException("用户名或密码未填写");
        CheckHelper.NotNull(userDto,"没有正确传入数据");
        
        //使用配置好的mapster来转换类型
        var user =userDto.Adapt<User>();

        //查看是否有重复用户名或者不存在对应的租户
        var username = _userRepository.GetFirst(a => a.Username == user.Username);
        if(username != null) throw new DomainException("用户名已存在,请更换用户名");
        var tenantId = _db.Queryable<Tenant>().Where(i => i.Id == user.TenantId).Count();
        if(tenantId==0) throw new DomainException("租户id不存在");


        // var TenantId = _httpContextAccessor.HttpContext?.User.FindFirstValue("tenantId");
        //获取当前userId并作为创建id赋值
        var UserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CheckHelper.NotNull(UserId, "当前用户信息不存在");
        user.CreateUserId = UserId ?? "1";
        user.Id = Guid.NewGuid().ToString();

        // //给user增加导航属性作用被mapster取代了
        // AddUserNavigationRole(ref user,userDto);
        var result = await _db.InsertNav(user)
        .Include(x => x.RoleList, new InsertNavOptions()
        {
            ManyToManyNoDeleteMap = true
        })
        .ExecuteCommandAsync();
        if(!result) throw new DomainException("插入失败");
        return $"恭喜成功插入，{user.Username}";
    }

    /// <summary>
    /// 获取全部用户信息
    /// </summary>
    /// <returns></returns>
    public async Task<List<UserVm>> GetAllUsersAsync()
    {
        var user = await  _db.Queryable<User>().Includes(a => a.RoleList).ToListAsync();
        var userDto = user.Adapt<List<UserVm>>();
        return userDto;
    }

    /// <summary>
    /// 获取分页User
    /// </summary>
    /// <param name="userQms"></param>
    /// <returns></returns>
    public async Task<PagedList<UserVm>> GetPagedUsersAsync(UserQms userQms)
    {
        RefAsync<int> totalCount = 0;
        var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CheckHelper.NotNull(userId,"当前用户不存在");
        CheckHelper.NotNull(await _userRepository.GetByIdAsync(userId),"当前用户信息不存在");

        //使用sqlsugarClient获取数据
        var userList = await _db.Queryable<User>()
        .WhereIF(!string.IsNullOrWhiteSpace(userQms.Name),it => it.Username.Contains(userQms.Name ?? "1"))
        .OrderBy(userQms.SortBy)
        .Includes(x => x.RoleList)
        .ToPageListAsync(userQms.PageNum,userQms.PageSize,totalCount);

        var userDtoList = userList.Adapt<List<UserVm>>();
        
        //生成适合返回给前端的格式
        var formattedRes = new PagedList<UserVm>(userDtoList,userQms.PageNum,userQms.PageSize,totalCount);
        return formattedRes;        
    }

    public async Task<List<MenuVm>> GetMenuList()
    {
        var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CheckHelper.NotNull(userId,"当前用户不存在");
        //获取当前用户对应的权限
        var roleIds = await _db.Queryable<UserRole>().Where(a => a.UserId == userId).Select(a => a.RoleId).ToListAsync();
        CheckHelper.NotNull(roleIds,"当前用户不存在对应的角色");
        var MenuIds = await _db.Queryable<RoleMenu>().Where(a => roleIds.Contains(a.RoleId ?? "0")).Select(a => a.MenuId).Distinct().ToListAsync();
        CheckHelper.NotNull(MenuIds,"当前角色没有任何权限");
        var menu = await _db.Queryable<Menu>().Where(a => MenuIds.Contains(a.Id)).ToListAsync();
                
        //先转换成listvm
        var menuVmList = menu.Adapt<List<MenuVm>>();
        GetMenuVms(menuVmList);

        return menuVmList;                    
    }

    #endregion

    #region 修改用户，删除用户
    public async Task<string> UpdateUserAsync(UserVm userVm)
    {
        if(string.IsNullOrWhiteSpace(userVm?.Username)) throw new DomainException("用户名未填写");

        var userCount = await _userRepository.AsQueryable().Where(user => user.Id == userVm.Id).AnyAsync();
        if(!userCount) throw new DomainException("用户不存在");
        var updateUser = userVm.Adapt<User>();
        updateUser.UpdateUserId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        //更新用户以及用户角色关系表
        var result =await  _db.UpdateNav(updateUser,new UpdateNavRootOptions(){
                        IsIgnoreAllNullColumns = true
                        })
                        .Include(a => a.RoleList, new UpdateNavOptions { 
                         ManyToManyIsUpdateA=true                         
                        })
                       .ExecuteCommandAsync();
        if (!result) throw new DomainException("更新失败，请查看更新数据格式是否正确");
        return "插入成功";
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="userVm"></param>
    /// <returns></returns>
    /// <exception cref="DomainException"></exception>
    public async Task<string> DeleteUserAsync(UserVm userVm)
    {
        if(string.IsNullOrWhiteSpace(userVm.Username)) throw new DomainException("用户名未填写");
        var userCount = _userRepository.AsQueryable().Where(user => user.Id == userVm.Id).Count();
        if(userCount == 0) throw new DomainException("删除用户不存在");
        var deleteUser = userVm.Adapt<User>();
        //删除
        deleteUser.IsDeleted =true;
        deleteUser.UpdateUserId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        deleteUser.UpdateTime = DateTime.Now;
        var res =_db.Updateable<User>().ExecuteCommandAsync();
        if (res != null) return "删除成功";
        throw new DomainException("删除失败");        
    }

    /// <summary>
    /// 返回当前用户信息
    /// </summary>
    /// <returns></returns>
    public async Task<InfoVm> GetInfoAsync()
    {
        var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CheckHelper.NotNull(userId,"当前用户不存在");
        
        var user =await _db.Queryable<User>().Includes(t => t.RoleList).FirstAsync(a => a.Id == userId);
        CheckHelper.NotNull(user,"当前用户信息不存在");
        var info = user.Adapt<InfoVm>();   
        return info;
    }

    /// <summary>
    /// 返回所有角色列表
    /// </summary>
    /// <returns></returns>
    /// <exception cref="DomainException"></exception>
    public async Task<List<RoleVm>> GetRoleListAsyc()
    {
        var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CheckHelper.NotNull(userId,"当前用户不存在");

        var userExist =await _db.Queryable<User>().AnyAsync(a => a.Id == userId);
        if (!userExist) throw new DomainException("当前用户不存在");

        var roles = await _db.Queryable<Role>().ToListAsync();
        var res = roles.Adapt<List<RoleVm>>();
        return res;
    }
    

   
    #endregion


    #region 通用方法    
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
    
    /// <summary>
    /// 获取所有菜单(树型)
    /// </summary>
    /// <param name="menus"></param>
    /// <param name="pid"></param>
    /// <returns></returns>
    public List<MenuVm> GetMenuVms( List<MenuVm> menus, string pid ="0" )
    {
        if(menus == null || !menus.Any(w => w.Pid == pid) ) return new List<MenuVm>();
        var res = new List<MenuVm>();
        var children = menus.Where(a => a.Pid == pid).ToList();
        foreach (var menu in children)
        {
            menu.Children = GetMenuVms(menus, menu.Id);
            res.Add(menu);
        } 
        return res;       
    }
    #endregion

    #region 废弃方法
    /// <summary>
    /// 给user增加导航属性
    /// </summary>
    /// <param name="user"></param>
    /// <param name="userDto"></param>
    public void AddUserNavigationRole (ref User user, UserDto userDto)
    {
        //初始化列表因为导航属性必须不能初始化
        user.RoleList=new List<Role>();
        // 使用sqlSugar导航属性来插入数据
        foreach (var id in userDto.RoleIdList)
        {            
            user.RoleList.Add(new Role
            {
                Id = id
            });            
        }        
    }
    #endregion
}
