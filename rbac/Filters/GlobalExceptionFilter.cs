using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using rbac.Infra;
using rbac.Infra.Exceptions;

namespace rbac.Filters;
/// <summary>
/// 全局错误拦截
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(IWebHostEnvironment env, ILogger<GlobalExceptionFilter> logger)
    {
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// 处理异常
    /// </summary>
    /// <param name="context"></param>
    public void OnException(ExceptionContext context)
    {
        _logger.LogError(new EventId(context.Exception.HResult),context.Exception,context.Exception.Message);

        if(context.Exception is DomainException domainException)
        {
            DomainExceptionHandler(context,domainException);
        }
        else
        {
            DefaultExceptionHandler(context);
        }

        

    }

    private void DefaultExceptionHandler(ExceptionContext context)
    {
        var details = new ExceptionResponse
        {
            Instance = context.HttpContext.Request.Path,
            Code = StatusCodes.Status500InternalServerError,
            Msg = "服务器繁忙请稍后再试",
            Errors=new Dictionary<string, object>()
        };

        if (_env?.IsProduction() == false)
            details.Errors.Add("Exception", context.Exception.ToString());
        
        context.Result = new ObjectResult(details);
        context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
    }

    private void DomainExceptionHandler(ExceptionContext context, DomainException domainException)
    {
        var details = new ExceptionResponse
        {
            Instance = context.HttpContext.Request.Path,
            Code = StatusCodes.Status400BadRequest,
            Msg=domainException.Message,
            Errors=new Dictionary<string, object>{{"BadRequest",new string[]{context.Exception.Message}}}
        };

        if (_env?.IsProduction() == false)
            details.Errors.Add("Exception", context.Exception.ToString());
        
        context.Result = new ObjectResult(details);
        context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
}
