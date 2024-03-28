using System;
using FluentValidation;
using FluentValidation.Results;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;

namespace WebApplication.Core.Common.CustomProblemDetails
{
    public class BadRequestProblemDetails : StatusCodeProblemDetails
    {
        /// <inheritdoc />
        public BadRequestProblemDetails(ValidationException ex) : base(StatusCodes.Status400BadRequest)
        {
            Detail = string.Join(';', ex.Errors);
        }

        /// <inheritdoc />
        public BadRequestProblemDetails(Exception ex) : base(StatusCodes.Status400BadRequest)
        {
            Detail = ex.Message;
        }

        public BadRequestProblemDetails(ValidationFailure ex) : base(StatusCodes.Status400BadRequest)
        {
            Detail = string.Join(';', ex.ErrorMessage);
        }
    }
}
