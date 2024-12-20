using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using rbac.Modals.Models;
using SqlSugar;

namespace rbac.CoreBusiness.Jobs;

public class ClearHistoryJob : IJob
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ClearHistoryJob> _logger;
    private readonly ISqlSugarClient _db;

    public ClearHistoryJob(IConfiguration configuration,ILogger<ClearHistoryJob> logger,ISqlSugarClient db)
    {
        _configuration = configuration;
        _logger = logger;
        _db = db;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogTrace("定时任务开始：{Description}", context.JobDetail.Description);

        string logsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");

        if (!Directory.Exists(logsFolderPath))
        {
            _logger.LogInformation("日志文件夹不存在！");
        }
        // 日志保留天数 LogFileMaintainTime
        int retentionDays = _configuration.GetValue<int>("LogFileMaintainTime");

        // 获取日志文件
        string[] logFiles = Directory.GetFiles(logsFolderPath, "*.txt");

        foreach (string logFile in logFiles)
        {
            FileInfo fileInfo = new FileInfo(logFile);

            // 如果文件修改时间早于保留期限，删除文件
            if (fileInfo.LastWriteTime <DateTime.Now.AddDays(-retentionDays))
            {
                File.Delete(logFile);
                _logger.LogInformation($"删除日志文件：{logFile}");
            }
        }
                
    }
}
