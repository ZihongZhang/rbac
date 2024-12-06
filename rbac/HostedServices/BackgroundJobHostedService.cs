using System;
using Quartz;
using rbac.CoreBusiness.Jobs;

namespace rbac.HostedServices;

public class BackgroundJobHostedService : IHostedService
{
    private readonly ILogger<BackgroundJobHostedService> _logger;
    private readonly ISchedulerFactory _factory;

    public BackgroundJobHostedService(ILogger<BackgroundJobHostedService> logger,ISchedulerFactory schedulerFactory)
    {
        _logger = logger;
        _factory = schedulerFactory;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // await AddJobSchedulerAsync<TestJob>("0/5 * * * * ? ", "1512", nameof(TestJob), "测试数据");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("后台服务--结束");
        return Task.CompletedTask;
    }


    private async Task AddJobSchedulerAsync<TJob>(string cron, string jobGroup, string jobName, string description) where TJob : IJob
    {
        var scheduler = await _factory.GetScheduler();

        var job = JobBuilder.Create<TJob>()
            .WithIdentity(jobName, jobGroup)
            .WithDescription(description)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(jobName, jobGroup)
            .WithDescription(description)
            .StartNow()
            .WithCronSchedule(cron)
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }
}
