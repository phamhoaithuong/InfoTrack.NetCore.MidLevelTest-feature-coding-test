using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace WebApplication.Core.Common.Behaviours
{
    public class RequestValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger<RequestValidationBehaviour<TRequest, TResponse>> _logger;

        public RequestValidationBehaviour(IEnumerable<IValidator<TRequest>> validators, ILogger<RequestValidationBehaviour<TRequest, TResponse>> logger)
        {
            _validators = validators;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            // TODO: throw a validation exception if there are any validation errors
            // NOTE: the validation exception should contain all failures
            _logger.LogInformation($"Handling {typeof(TRequest).Name}");
            var stopwatch = Stopwatch.StartNew();
            TResponse response;

            try
            {
                var context = new ValidationContext<TRequest>(request);

                var validationFailures = await Task.WhenAll(
                    _validators.Select(validator => validator.ValidateAsync(context)));

                var errors = validationFailures
                    .Where(validationResult => !validationResult.IsValid)
                    .SelectMany(validationResult => validationResult.Errors)
                    .ToList();

                if (errors.Any())
                {
                    throw new ValidationException(errors);
                }

                response = await next();
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation(
                    $"Handled {typeof(TRequest).Name}; Execution time = {stopwatch.ElapsedMilliseconds}ms");
            }
            return response;

        }
    }
}
