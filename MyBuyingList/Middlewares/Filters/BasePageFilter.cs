using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBuyingList.Application.Common.Exceptions;
using MyBuyingList.Web.Pages;
using MyBuyingList.Web.Pages.Shared;

namespace MyBuyingList.Web.Middlewares.Filters;

public class BasePageFilter : IAsyncPageFilter
{
    private readonly ILogger<BasePageFilter> _logger;

    public BasePageFilter(ILogger<BasePageFilter> logger)
    {
        _logger = logger;
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new PageResult();
            return;
        }

        PageHandlerExecutedContext result = await next();

        if (result.Exception is null
            || result.ExceptionHandled
            || result.Exception is not IFormattedResponseException formattedException)
        {
            return;
        }

        result.ExceptionHandled = true;

        LogException(result.Exception, formattedException.StatusCode);

        if (formattedException.Error is null || context.HandlerInstance is not BasePageModel pageModel)
        {
            result.Result = new RedirectToPageResult("/Error");
            return;
        }

        ErrorDetail error = formattedException.Error.Errors[0];
        pageModel.SetError(error.Title, error.Detail);
        result.Result = new PageResult();
    }

    private void LogException(Exception exception, int statusCode)
    {
        if (statusCode >= 500)
            _logger.LogError(exception, "Server error handling request. StatusCode: {StatusCode}", statusCode);
        else
            _logger.LogWarning("Client error handling request. StatusCode: {StatusCode}", statusCode);
    }
}
