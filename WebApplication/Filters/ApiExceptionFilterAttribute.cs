using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using WebApplication.Core.Common.CustomProblemDetails;
using Microsoft.AspNetCore.Http;

namespace WebApplication.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;
        private readonly ILogger<ApiExceptionFilterAttribute> _logger;

        public ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> logger)
        {
            _logger = logger;

            _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>> {
                {
                    typeof (ValidationException), HandleValidationException
                },
            };
        }

        private void HandleValidationException(ExceptionContext context)
        {
            var trueException = context.Exception as ValidationException;
            var details = new BadRequestProblemDetails(trueException);
            context.Result = new BadRequestObjectResult(details);
            context.ExceptionHandled = true;

            _logger.LogError("Validation exception:\n{Exception}", context.Exception);
        }

        public override void OnException(ExceptionContext context)
        {
            HandleException(context);
            base.OnException(context);
        }

        private void HandleException(ExceptionContext context)
        {
            Type type = context.Exception.GetType();
            if (_exceptionHandlers.ContainsKey(type))
            {
                _exceptionHandlers[type].Invoke(context);
                return;
            }

            _logger.LogError("Exception:\n{Exception}", context.Exception);
        }
    }
}