using FluentValidation;
namespace MyWeather.WebApi;

public static class ValidationFilterExtension
{
    public static RouteHandlerBuilder WithValidationFilter<TRequest>(this RouteHandlerBuilder builder)
        where TRequest : notnull
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
            if (request is null)
            {
                return await next(context);
            }

            if (context.HttpContext.RequestServices.GetService(typeof(IValidator<TRequest>)) is IValidator<TRequest> validator)
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                    return Results.BadRequest(errors);
                }
            }

            return await next(context);
        });
    }
    
}