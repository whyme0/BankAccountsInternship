using Api.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Filters;

// ReSharper disable once ClassNeverInstantiated.Global
public class MbResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: IMbResult mbResult })
        {
            context.HttpContext.Response.StatusCode = mbResult.StatusCode;
        }

        await next();
    }
}