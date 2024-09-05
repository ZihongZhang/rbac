﻿using rbac.Infra.Helper;
using rbac.Modals.Enum;
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
            db.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Menu), typeof(Role), typeof(RoleMenu), typeof(Tenant), typeof(User), typeof(UserRole));

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
                    new Menu{ CreateUserId="0", Id="1310000000101", Pid=0, Title="系统管理", Path="/system", Name="system", Component="Layout", Icon="ele-Setting", Type=MenuTypeEnum.Dir, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
                    new Menu{ CreateUserId="0", Id="1310000000111", Pid=1310000000101, Title="账号管理", Path="/system/user", Name="sysUser", Component="/system/user/index", Icon="ele-User", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
                    new Menu{ CreateUserId="0", Id="1310000000112", Pid=1310000000111, Title="查询", Permission="sysUser:page", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
                    new Menu{ CreateUserId="0", Id="1310000000113", Pid=1310000000111, Title="编辑", Permission="sysUser:update", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
                    new Menu{ CreateUserId="0", Id="1310000000114", Pid=1310000000111, Title="增加", Permission="sysUser:add", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
                    new Menu{ CreateUserId="0", Id="1310000000115", Pid=1310000000111, Title="删除", Permission="sysUser:delete", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
                    new Menu{ CreateUserId="0", Id="1310000000116", Pid=1310000000111, Title="详情", Permission="sysUser:detail", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
                    new Menu{ CreateUserId="0", Id="1310000000117", Pid=1310000000111, Title="授权角色", Permission="sysUser:grantRole", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
                    new Menu{ CreateUserId="0", Id="1310000000118", Pid=1310000000111, Title="重置密码", Permission="sysUser:resetPwd", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
                    new Menu{ CreateUserId="0", Id="1310000000119", Pid=1310000000111, Title="设置状态", Permission="sysUser:setStatus", Type=MenuTypeEnum.Btn, CreateTime=DateTime.Parse("2022-02-10 00:00:00") }       
                };
                db.Insertable(Menus).ExecuteCommand();
            }
            if (!db.Queryable<Tenant>().Any())
            {
                // 创建权限种子数据
                var Tenants = new List<Tenant>
                {
                    new Tenant { Id="0",TenantName = "FirstSuperAdminTenant", TenantType=TenantTypeEnum.Id, IsDeleted = false, UpdateTime = DateTime.Now,CreateUser ="systemDesigner",CreateUserId="0" }     
                };
                db.Insertable(Tenants).ExecuteCommand();
            }

            if (!db.Queryable<Role>().Any())
            {
                // 创建角色种子数据
                roles = new List<Role>
                {
                    new Role { Id=Guid.NewGuid().ToString(),RoleName = "Administrator", ParentRoleId = "0",CreateUserId="0" }
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
                    new RoleMenu { RoleId = roles[0].Id, MenuId="1310000000116"},
                    new RoleMenu { RoleId = roles[0].Id, MenuId="1310000000117"},
                    new RoleMenu { RoleId = roles[0].Id, MenuId="1310000000118"},                    
                    new RoleMenu { RoleId = roles[0].Id, MenuId="1310000000118"}                    
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
            
        }
    }
}