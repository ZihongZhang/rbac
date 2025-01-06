using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;


namespace rbac.Filters;

public class ArgumentsFilter : IAsyncActionFilter
{
    private readonly ILogger<ArgumentsFilter> _logger;

    public ArgumentsFilter(ILogger<ArgumentsFilter> logger)
    {
        _logger = logger;
    }
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var arguments = JsonConvert.SerializeObject(context.ActionArguments);
        _logger.LogInformation("{arguments:l}", arguments);
        await next();
    }
}
