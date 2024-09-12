using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using rbac.Infra;

namespace rbac.Filters;

public class GlobalResultFilter : IAlwaysRunResultFilter
{
    public void OnResultExecuted(ResultExecutedContext context)
    {
        
    }
    /// <summary>
    /// 规范返回格式，默认返回直接是200
    /// </summary>
    /// <param name="context"></param>
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not RedirectResult
                && context.Result is not RedirectToActionResult
                && context.Result is not FileResult)
            {
                if (context.Result is ObjectResult result)
                {
                    if (result.Value is not IResponse)
                    {
                        var detail = new
                        {
                            Code = result.StatusCode ?? 200,
                            Msg = (result.StatusCode ?? 200) == StatusCodes.Status200OK ? "操作成功" : "操作失败",
                            Data = result.Value,
                        };
                        context.Result = new ObjectResult(detail);
                    }
                }
                else if (context.Result is StatusCodeResult codeResult)
                {
                    var detail = new
                    {
                        Code = codeResult.StatusCode,
                        Msg = context.HttpContext.Response.StatusCode == StatusCodes.Status200OK ? "操作成功" : "操作失败",
                    };
                    context.Result = new ObjectResult(detail);
                }
                else if (context.Result is ActionResult)
                {
                    var detail = new
                    {
                        Code = context.HttpContext.Response.StatusCode,
                        Msg = context.HttpContext.Response.StatusCode == StatusCodes.Status200OK ? "操作成功" : "操作失败",
                    };
                    context.Result = new ObjectResult(detail);
                }
            }
            context.HttpContext.Response.StatusCode = 200;
    }
}
