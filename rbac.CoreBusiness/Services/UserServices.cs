using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Castle.Core.Logging;
using Mapster;
using MapsterMapper;
using Masuit.Tools;
using Masuit.Tools.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<UserServices> _logger;

    // private readonly IMapper _mapper;

    public Repository<User> _userRepository { get; }
    public ISqlSugarClient _db { get; }
    public IHttpContextAccessor _httpContextAccessor { get; }

    public UserServices(Repository<User> repository, ISqlSugarClient db, IHttpContextAccessor httpContextAccessor,ILogger<UserServices> logger)
    {
        _userRepository = repository;
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
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
        _logger.LogInformation("登录信息：{@loginDto}", loginDto);

        if (string.IsNullOrEmpty(loginDto.Password?.Trim()))
        {
            throw new DomainException("密码不能为空!");
        }
        User user = null;
        
        lock (_db) 
        {
            //var user = await _db.Queryable<User>().Where(a => a.Username == loginDto.UserName).FirstAsync()
            //    ?? throw new DomainException("不存在该用户");
            user =  _db.Queryable<User>().Where(a => a.Username == loginDto.UserName).First()
               ?? throw new DomainException("不存在该用户");
           
        }
        if (user.Status == StatusEnum.Disable) throw new DomainException("该用户已被禁用");

        if (!user.Password.Equals(loginDto.Password))
        {
            throw new DomainException("登录失败");
        }

        return GenerateToken(user);
    }

    /// <summary>
    /// 添加新用户
    /// </summary>
    /// <param name="userDto"></param>
    /// <returns></returns>
    public async Task<string> AddUserAsync(UserDto userDto)
    {
        _logger.LogInformation("登录信息：{@userDto}", userDto);
        if (string.IsNullOrWhiteSpace(userDto.Username)
        || string.IsNullOrWhiteSpace(userDto.Password)
        || string.IsNullOrWhiteSpace(userDto.Email))
            throw new DomainException("用户名或密码或邮箱未填写");

        //使用配置好的mapster来转换类型
        var user = userDto.Adapt<User>();

        //查看是否有重复用户名或者不存在对应的租户
        var username = _userRepository.GetFirst(a => a.Username == user.Username);
        if (username != null) throw new DomainException("用户名已存在,请更换用户名");
        var tenantId = await _db.Queryable<Tenant>().Where(i => i.Id == user.TenantId).CountAsync();
        if (tenantId == 0) throw new DomainException("租户id不存在");


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
        if (!result) throw new DomainException("插入失败");
        return $"恭喜成功插入，{user.Username}";
    }

    /// <summary>
    /// 获取全部用户信息
    /// </summary>
    /// <returns></returns>
    public async Task<List<UserVm>> GetAllUsersAsync()
    {
        var user = await _db.Queryable<User>().Includes(a => a.RoleList).ToListAsync();
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
        CheckHelper.NotNull(userId, "当前用户不存在");
        CheckHelper.NotNull(await _userRepository.GetByIdAsync(userId), "当前用户信息不存在");

        //使用sqlsugarClient获取数据
        var userList =await _db.Queryable<User>()
        .WhereIF(!string.IsNullOrWhiteSpace(userQms.Name), it => it.Username.Contains(userQms.Name ?? "1"))
        .OrderBy(userQms.SortBy)
        .Includes(x => x.RoleList)
        .ToPageListAsync(userQms.PageNum, userQms.PageSize, totalCount);

        var userDtoList = userList.Adapt<List<UserVm>>();

        //生成适合返回给前端的格式
        var formattedRes = new PagedList<UserVm>(userDtoList, userQms.PageNum, userQms.PageSize, totalCount);
        return formattedRes;
    }

    public async Task<List<MenuVm>> GetMenuList()
    {
        var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CheckHelper.NotNull(userId, "当前用户不存在");
        //获取当前用户对应的权限
        var roleIds = await _db.Queryable<UserRole>().Where(a => a.UserId == userId).Select(a => a.RoleId).ToListAsync();
        CheckHelper.NotNull(roleIds, "当前用户不存在对应的角色");
        var MenuIds = await _db.Queryable<RoleMenu>().Where(a => roleIds.Contains(a.RoleId ?? "0")).Select(a => a.MenuId).Distinct().ToListAsync();
        CheckHelper.NotNull(MenuIds, "当前角色没有任何权限");
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
        if (string.IsNullOrWhiteSpace(userVm?.Username)) throw new DomainException("用户名未填写");

        var userCount = await _userRepository.AsQueryable().Where(user => user.Id == userVm.Id).AnyAsync();
        if (!userCount) throw new DomainException("用户不存在");
        var updateUser = userVm.Adapt<User>();
        updateUser.UpdateUserId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        //更新用户以及用户角色关系表
        var result = await _db.UpdateNav(updateUser, new UpdateNavRootOptions()
        {
            IsIgnoreAllNullColumns = true
        })
                        .Include(a => a.RoleList, new UpdateNavOptions
                        {
                            ManyToManyIsUpdateA = true
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
        var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CheckHelper.NotNull(userId, "当前用户不存在");
        if (string.IsNullOrWhiteSpace(userVm.Username)) throw new DomainException("用户名未填写");
        var userCount = _userRepository.AsQueryable().Where(user => user.Id == userVm.Id).Count();
        if (userCount == 0) throw new DomainException("删除用户不存在");
        var deleteUser = userVm.Adapt<User>();
        //删除
        deleteUser.IsDeleted = true;
        deleteUser.UpdateUserId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        deleteUser.UpdateTime = DateTime.Now;
        var res = await _db.Updateable<User>().SetColumns(a => new User()
        {
            IsDeleted = true,
            UpdateTime = DateTime.Now,
            UpdateUserId = userId
        })
            .Where(a => a.Id == userVm.Id)
            .ExecuteCommandAsync();
        if (res != 0) return "删除成功";
        throw new DomainException("删除失败");
    }

    /// <summary>
    /// 返回当前用户信息
    /// </summary>
    /// <returns></returns>
    public async Task<InfoVm> GetInfoAsync()
    {
        var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CheckHelper.NotNull(userId, "当前用户不存在");

        var user = await _db.Queryable<User>().Includes(t => t.RoleList).FirstAsync(a => a.Id == userId);
        CheckHelper.NotNull(user, "当前用户信息不存在");
        var info = user.Adapt<InfoVm>();
        return info;
    }
    #endregion

    #region 角色相关方法
    /// <summary>
    /// 获取所有角色信息
    /// </summary>
    /// <returns></returns>
    public async Task<List<RoleVm>> GetRoleListAsyc()
    {
        var roles = await _db.Queryable<Role>()
        .Includes(a => a.MenuList)
        .ToListAsync();
        var res = roles.Adapt<List<RoleVm>>();
        return res;
    }
    /// <summary>
    /// 修改角色信息
    /// </summary>
    /// <param name="roleVms"></param>
    /// <returns></returns>
    public async Task<string> UpdateRolesMenuAsync(RoleVm roleVms)
    {
        var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CheckHelper.NotNull(userId, "当前用户不存在");
        if (string.IsNullOrWhiteSpace(roleVms.RoleName) || string.IsNullOrWhiteSpace(roleVms.Id) || string.IsNullOrWhiteSpace(roleVms.ParentRoleId))
            throw new DomainException("角色信息未填写完整");
        var userExist = await _db.Queryable<User>().AnyAsync(a => a.Id == userId);
        if (!userExist) throw new DomainException("当前用户不存在");
        var roleExist = await _db.Queryable<Role>().AnyAsync(a => a.Id == roleVms.Id);
        if (!roleExist) throw new DomainException("当前用户不存在");
        var role = roleVms.Adapt<Role>();
        var res = await _db.UpdateNav(role, new UpdateNavRootOptions()
        {
            IsIgnoreAllNullColumns = true
        })
                    .Include(a => a.MenuList, new UpdateNavOptions
                    {
                        ManyToManyIsUpdateA = true
                    })
                    .ExecuteCommandAsync();
        if (res) return "更新成功";
        throw new DomainException("更新角色权限失败失败");
    }
    /// <summary>
    /// 添加新角色信息
    /// </summary>
    /// <param name="roleVm"></param>
    /// <returns></returns>
    public async Task<string> AddRoleAsync(RoleVm roleVm)
    {
        var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CheckHelper.NotNull(userId, "当前用户不存在");
        var userExist = await _db.Queryable<User>().AnyAsync(a => a.Id == userId);
        if (!userExist) throw new DomainException("当前用户不存在");
        if (string.IsNullOrWhiteSpace(roleVm.RoleName) || string.IsNullOrWhiteSpace(roleVm.Id) || string.IsNullOrWhiteSpace(roleVm.ParentRoleId))
            throw new DomainException("角色信息未填写完整");
        var role = roleVm.Adapt<Role>();
        role.Id = Guid.NewGuid().ToString();
        role.CreateUserId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var res = await _db.InsertNav(role).Include(x => x.MenuList).ExecuteCommandAsync();
        if (res) return "插入成功";
        throw new DomainException("插入失败");
    }
    /// <summary>
    /// 删除角色信息
    /// </summary>
    /// <param name="roleVm"></param>
    /// <returns></returns>
    public async Task<string> DeleteRoleAsync(RoleVm roleVm)
    {
        var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        CheckHelper.NotNull(userId, "当前用户不存在");
        var userExist = await _db.Queryable<User>().AnyAsync(a => a.Id == userId);
        if (!userExist) throw new DomainException("当前用户不存在");
        if (string.IsNullOrWhiteSpace(roleVm.RoleName) || string.IsNullOrWhiteSpace(roleVm.Id) || string.IsNullOrWhiteSpace(roleVm.ParentRoleId))
            throw new DomainException("角色信息未填写完整");
        var res = await _db.Updateable<Role>().SetColumns(a => new Role()
        {
            IsDeleted = true,
            UpdateTime = DateTime.Now,
            UpdateUserId = userId
        })
        .Where(a => a.Id == roleVm.Id).ExecuteCommandAsync();
        if (res != 0) return "删除成功";
        throw new DomainException("删除失败");
    }


    #endregion
    #region 通用方法    
    /// <summary>
    /// 产生token
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private string GenerateToken(User user)
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
    private List<MenuVm> GetMenuVms(List<MenuVm> menus, string pid = "0")
    {
        //if (menus == null || !menus.Any(w => w.Pid == pid)) return new List<MenuVm>();
        var res = new List<MenuVm>();
        //获取所有子节点
        var children = menus.Where(a => a.Pid == pid).ToList();
        //遍历所有子节点
        foreach (var menu in children)
        {
            //查看子节点中是否有新的子节点
            menu.Children = GetMenuVms(menus, menu.Id);
            //将节点挂在树上
            res.Add(menu);
        }
        //返回最终结果
        return res;
    }
    #endregion

    #region 废弃方法
    /// <summary>
    /// 给user增加导航属性
    /// </summary>
    /// <param name="user"></param>
    /// <param name="userDto"></param>
    private void AddUserNavigationRole(ref User user, UserDto userDto)
    {
        //初始化列表因为导航属性必须不能初始化
        user.RoleList = new List<Role>();
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
