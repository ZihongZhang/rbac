using rbac.Infra.Helper;
using rbac.Modals.AggregateRoots;
using rbac.Modals.Models;
using SqlSugar;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace rbac.StartupExtensions
{
    public static class SqlSugarSetup
    {
        public static void AddSqlsugarSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            //添加数据库连接
            SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = "datasource=demo.db",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
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
            db.DbMaintenance.CreateDatabase(Directory.GetCurrentDirectory());
            db.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Permission), typeof(Role), typeof(RolePermission), typeof(Tenant), typeof(User), typeof(UserRole));

            //提前定义来然后面可以拿到数据
            List<Permission>? permissions = null;
            List<Role>? roles = null;
            List<User>? users = null;

            // 检查是否已有数据
            if (!db.Queryable<Permission>().Any())
            {
                // 创建权限种子数据
                permissions = new List<Permission>
                {
                    new Permission { PermissionName = "Create", IsEnabled = true, TenantId = "0", IsDeleted = false, UpdateTime = DateTime.Now },
                    new Permission { PermissionName = "Read", IsEnabled = true, TenantId = "0", IsDeleted = false, UpdateTime = DateTime.Now },
                    new Permission { PermissionName = "Update", IsEnabled = true, TenantId = "0", IsDeleted = false, UpdateTime = DateTime.Now },
                    new Permission { PermissionName = "Delete", IsEnabled = true, TenantId = "0", IsDeleted = false, UpdateTime = DateTime.Now }
                };
                db.Insertable(permissions).ExecuteCommand();
            }
            if (!db.Queryable<Tenant>().Any())
            {
                // 创建权限种子数据
                var Tenants = new List<Tenant>
                {
                    new Tenant { TenantName = "FirstSuperAdminTenant", IsEnabled = true, TenantId = "0", IsDeleted = false, UpdateTime = DateTime.Now,CreateUser ="0" },
                    new Tenant { TenantName = "secondTenant", IsEnabled = true,  IsDeleted = false, UpdateTime = DateTime.Now,CreateUser ="0" }
                };
                db.Insertable(Tenants).ExecuteCommand();
            }

            if (!db.Queryable<Role>().Any())
            {
                // 创建角色种子数据
                roles = new List<Role>
                {
                    new Role { RoleId="100",RoleName = "Administrator", IsEnabled = true, TenantId = "0", IsDeleted = false, UpdateTime = DateTime.Now, ParentRoleId = "0" },
                    new Role { RoleName = "User", IsEnabled = true, TenantId = "0", IsDeleted = false, UpdateTime = DateTime.Now, ParentRoleId = "100" }
                };
                db.Insertable(roles).ExecuteCommand();
            }

            if (!db.Queryable<RolePermission>().Any())
            {
                // 创建角色权限关联数据
                var rolePermissions = new List<RolePermission>
                {
                    new RolePermission { PermissionId = permissions[0].PermissionId, RoleId = roles[0].RoleId, TenantId = "0", IsDeleted = false },
                    new RolePermission { PermissionId = permissions[1].PermissionId, RoleId = roles[0].RoleId, TenantId = "0", IsDeleted = false },
                    new RolePermission { PermissionId = permissions[2].PermissionId, RoleId = roles[0].RoleId, TenantId = "0", IsDeleted = false },
                    new RolePermission { PermissionId = permissions[3].PermissionId, RoleId = roles[0].RoleId, TenantId = "0", IsDeleted = false },
                    new RolePermission { PermissionId = permissions[2].PermissionId, RoleId = roles[1].RoleId, TenantId = "0", IsDeleted = false }
                };
                db.Insertable(rolePermissions).ExecuteCommand();
            }
            if (!db.Queryable<User>().Any())
            {
                // 创建角色权限关联数据
                users = new List<User>
                {
                    new User { UserId="0", TenantId="0",Username="SuperAdmin",Password=MD5Helper.CreateMd5("111"),Email="test@qq.com",IsEnabled=true,UpdateUserId="0"},
                    new User { TenantId="0",Username="Admin",Password=MD5Helper.CreateMd5("1111"),Email="test@qq.com",IsEnabled=true,UpdateUserId="0"  }
                };
                db.Insertable(users).ExecuteCommand();
            }
            if (!db.Queryable<UserRole>().Any())
            {
                // 创建角色权限关联数据
                var UserRoles = new List<UserRole>
                {
                    new UserRole { UserId = users[0].UserId, RoleId = roles[0].RoleId, TenantId = "0", IsDeleted = false ,CreateUser="0" },
                    new UserRole { UserId = users[1].UserId, RoleId = roles[1].RoleId, TenantId = "0", IsDeleted = false ,CreateUser="0"}
                };
                db.Insertable(UserRoles).ExecuteCommand();
            }
        }
    }
}
