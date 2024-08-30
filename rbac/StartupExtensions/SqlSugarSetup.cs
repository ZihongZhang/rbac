using rbac.Modals.Models;
using SqlSugar;
using System.Runtime.CompilerServices;

namespace rbac.StartupExtensions
{
    public static class SqlSugarSetup
    {
        public static void AddSqlsugarSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            //添加数据库连接
            SqlSugarClient Db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = "datasource=demo.db",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true
            },
            db =>
            {
                db.Aop.OnLogExecuting = (sql, parse) =>
                {
                    //将原生日志打印到控制台中
                    Console.WriteLine(UtilMethods.GetNativeSql(sql,parse));
                };
            });
            Db.DbMaintenance.CreateDatabase(Directory.GetCurrentDirectory());
            Db.CodeFirst.SetStringDefaultLength(200).InitTables(typeof(Permission),typeof(Role),typeof(RolePermission),typeof(Tenant),typeof(User),typeof(UserRole));

        }

    }
}
