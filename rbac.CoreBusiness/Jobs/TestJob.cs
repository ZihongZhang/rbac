using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using rbac.Modals.Models;
using SqlSugar;

namespace rbac.CoreBusiness.Jobs;

public class TestJob : IJob
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TestJob> _logger;
    private readonly ISqlSugarClient _db;

    public TestJob(IConfiguration configuration,ILogger<TestJob> logger,ISqlSugarClient db)
    {
        _configuration = configuration;
        _logger = logger;
        _db = db;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogTrace("定时任务开始：{Description}", context.JobDetail.Description);

        var res = await _db.Queryable<User>().ToListAsync();
        Console.WriteLine("测试定时任务正在执行");        
    }
}
