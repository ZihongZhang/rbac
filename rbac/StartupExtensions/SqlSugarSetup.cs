using rbac.Infra.Helper;
using rbac.Modals.Enum;
using rbac.Modals.Models;
using rbac.Modals.Models.AiRelatedModel;
using SqlSugar;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace rbac.StartupExtensions
{
    public static class SqlSugarSetup
    {
        public static IConfiguration _configuration { get; private set; }

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public static void AddSqlsugarSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            var dbs = _configuration["DBS:0:Connection"];
            string dbTypeString = _configuration["DBS:0:DBType"];
            DbType type = DbType.Sqlite;
            if (int.TryParse(dbTypeString, out int dbTypeValue))
            {
                // 2. 将整数值转换为 DbType 枚举
                type = (DbType)dbTypeValue;
            }
            else
            {
                throw new Exception("数据库类型配置错误");
            }

            #region 设置数据库连接
            //添加数据库连接
            SqlSugarScope db = new SqlSugarScope(new ConnectionConfig()
            {
                ConnectionString = dbs,
                DbType = type,
                IsAutoCloseConnection = true,
                MoreSettings = new ConnMoreSettings()
                {
                    DisableNvarchar = true //这里设置为true
                },
                ConfigureExternalServices = new ConfigureExternalServices
                {
                    //打开以下注释来让所有的表可为空
                    // //注意:  这儿AOP设置不能少,设置所有表的值可为空
                    // EntityService = (c, p) =>
                    // {
                    //     /***高版C#写法***/
                    //     //支持string?和string  
                    //     if (p.IsPrimarykey == false && new NullabilityInfoContext()
                    //     .Create(c).WriteState is NullabilityState.Nullable)
                    //     {
                    //         p.IsNullable = true;
                    //     }
                    // },
                    //将表全部驼峰转下划线
                    EntityNameService = (x, p) => //处理表名
                    {
                        //最好排除DTO类
                        p.DbTableName = UtilMethods.ToUnderLine(p.DbTableName);//驼峰转下划线方法
                    }
                }
            },

            db =>
            {
                db.Aop.OnLogExecuting = (sql, parse) =>
                {
                    //将原生日志打印到控制台中
                    Console.WriteLine(UtilMethods.GetNativeSql(sql, parse));
                };
                //默认查询只会查询没被软删除的内容
                db.QueryFilter.AddTableFilter<ISoftDeletable>(it => it.IsDeleted == false);
            });
            #endregion 

            #region 初始化表并加入种子数据
            db.DbMaintenance.CreateDatabase(Directory.GetCurrentDirectory());
            db.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Menu), typeof(Role), typeof(RoleMenu), typeof(Tenant), typeof(User), typeof(UserRole),typeof(MessageEntry),typeof(AiHistoryMessage));

            //提前定义来然后面可以拿到数据
            List<Menu>? Menus = null;
            List<Role>? roles = null;
            List<User>? users = null;

            // 检查是否已有数据
            if (!db.Queryable<Menu>().Any())
            {
                // 创建权限种子数据
                Menus = new List<Menu>
                {
                    new Menu{ CreateUserId="0", Id="1310000000101", Pid=0, Title="系统管理", Path="/system", Name="system", Component="Layout", Icon="ele-Setting", Type=MenuTypeEnum.Dir },
                    new Menu{ CreateUserId="0", Id="1310000000111", Pid=1310000000101, Title="账号管理", Path="/system/user", Name="sysUser", Component="/system/user/index", Icon="ele-User", Type=MenuTypeEnum.Menu },
                    new Menu{ CreateUserId="0", Id="1310000000112", Pid=1310000000111, Title="查询", Permission="sysUser:page", Type=MenuTypeEnum.Btn },
                    new Menu{ CreateUserId="0", Id="1310000000113", Pid=1310000000111, Title="编辑", Permission="sysUser:update", Type=MenuTypeEnum.Btn },
                    new Menu{ CreateUserId="0", Id="1310000000114", Pid=1310000000111, Title="增加", Permission="sysUser:add", Type=MenuTypeEnum.Btn },
                    new Menu{ CreateUserId="0", Id="1310000000115", Pid=1310000000111, Title="删除", Permission="sysUser:delete", Type=MenuTypeEnum.Btn },
                    new Menu{ CreateUserId="0", Id="1310000000118", Pid=1310000000111, Title="重置密码", Permission="sysUser:resetPwd", Type=MenuTypeEnum.Btn },
                    new Menu{ CreateUserId="0", Id="1310000000120", Pid=1310000000101, Title="账号管理", Path="/system/role", Name="sysRole", Component="/system/role/index", Icon="ele-User", Type=MenuTypeEnum.Menu },
                    new Menu{ CreateUserId="0", Id="1310000000121", Pid=1310000000120, Title="添加角色", Permission="sysUser:insertRole", Type=MenuTypeEnum.Btn },
                    new Menu{ CreateUserId="0", Id="1310000000122", Pid=1310000000120, Title="修改角色", Permission="sysUser:updateRole", Type=MenuTypeEnum.Btn },
                    new Menu{ CreateUserId="0", Id="1310000000123", Pid=1310000000120, Title="删除角色", Permission="sysUser:deleteRole", Type=MenuTypeEnum.Btn },
                    new Menu{ CreateUserId="0", Id="1310000000119", Pid=1310000000120, Title="查询角色", Permission="sysUser:getRole", Type=MenuTypeEnum.Btn }
                };
                db.Insertable(Menus).ExecuteCommand();
            }
            if (!db.Queryable<Tenant>().Any())
            {
                // 创建权限种子数据
                var Tenants = new List<Tenant>
                {
                    new Tenant { Id="0", TenantName = "FirstSuperAdminTenant", TenantType=TenantTypeEnum.Id, IsDeleted = false, UpdateTime = DateTime.Now,CreateUser ="systemDesigner",CreateUserId="0" }
                };
                db.Insertable(Tenants).ExecuteCommand();
            }

            if (!db.Queryable<Role>().Any())
            {
                // 创建角色种子数据
                roles = new List<Role>
                {
                    new Role { Id = "1", RoleName = "SuperAdministrator", ParentRoleId = "0",CreateUserId="0" },
                    new Role { Id = "2", RoleName = "Administrator", ParentRoleId="1", CreateUserId="0"}
                };
                db.Insertable(roles).ExecuteCommand();
            }

            if (!db.Queryable<RoleMenu>().Any())
            {
                // 创建角色权限关联数据
                var RoleMenus = new List<RoleMenu>
                {
                    new RoleMenu { RoleId = roles[0].Id, MenuId="1310000000101" },
                    new RoleMenu { RoleId = roles[0].Id, MenuId="1310000000111"},
                    new RoleMenu { RoleId = roles[0].Id, MenuId="1310000000112"},
                    new RoleMenu { RoleId = roles[0].Id, MenuId="1310000000113"},
                    new RoleMenu { RoleId = roles[0].Id, MenuId="1310000000114"},
                    new RoleMenu { RoleId = roles[0].Id, MenuId="1310000000115"},
                    new RoleMenu { RoleId = roles[0].Id, MenuId="1310000000118"},
                    new RoleMenu { RoleId = roles[0].Id, MenuId="1310000000120"},
                    new RoleMenu { RoleId = roles[0].Id, MenuId="1310000000121"},
                    new RoleMenu { RoleId = roles[0].Id, MenuId="1310000000122"},
                    new RoleMenu { RoleId = roles[0].Id, MenuId="1310000000123"},
                    new RoleMenu { RoleId = roles[0].Id, MenuId="1310000000124"},
                    new RoleMenu { RoleId = roles[1].Id, MenuId="1310000000101"},
                    new RoleMenu { RoleId = roles[1].Id, MenuId="1310000000111"},
                    new RoleMenu { RoleId = roles[1].Id, MenuId="1310000000112"},
                    new RoleMenu { RoleId = roles[1].Id, MenuId="1310000000119"}
                };
                db.Insertable(RoleMenus).ExecuteCommand();
            }

            if (!db.Queryable<User>().Any())
            {
                // 创建角色权限关联数据
                users = new List<User>
                {
                    new User { TenantId="0",Username="张三",Password=MD5Helper.CreateMd5("111"),Email="test@qq.com",CreateUserId="0"},
                    new User { TenantId="0",Username="李四",Password=MD5Helper.CreateMd5("1111"),Email="test1@qq.com",CreateUserId="0" }
                };
                db.Insertable(users).ExecuteCommand();
            }

            if (!db.Queryable<UserRole>().Any())
            {
                // 创建角色权限关联数据
                var UserRoles = new List<UserRole>
                {
                    new UserRole { UserId = users[0].Id, RoleId = roles[0].Id},
                    new UserRole { UserId = users[1].Id, RoleId = roles[0].Id}
                };
                db.Insertable(UserRoles).ExecuteCommand();
            }

            if (!db.Queryable<RoleMenu>().Any())
            {
                // 创建角色权限关联数据
                var UserRoles = new List<UserRole>
                {
                    new UserRole { UserId = users[0].Id, RoleId = roles[0].Id},
                    new UserRole { UserId = users[1].Id, RoleId = roles[0].Id}
                };
                db.Insertable(UserRoles).ExecuteCommand();
            }
            #endregion

            #region sqlsugar依赖注入
            //将sqlsugerclient注入
            services.AddSingleton<ISqlSugarClient>(o =>
            { return db; });
            #endregion
        }
    }
}
