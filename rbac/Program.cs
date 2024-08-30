
using rbac.Infra;
using rbac.StartupExtensions;

namespace rbac
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //将配置文件装到静态类中，从而在静态类中可以获取静态文件 尚未完成
            builder.Services.AddSingleton(new AppSetting(builder.Configuration));

            //添加sqlsugar设置并创建数据库
            
            builder.Services.AddSqlsugarSetup();
            
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
