using Microsoft.AspNetCore.Mvc;
using ListingSearch.Api.Middleware;
using ListingSearch.Api.Repositories;
using ListingSearch.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers provide a small, explicit HTTP API that is easy to understand and test.
builder.Services.AddControllers();
// Keep the data-access, scoring, and search responsibilities separate without introducing
// unnecessary infrastructure for a 12-record sample data set.
builder.Services.AddSingleton<IListingRepository, JsonListingRepository>();
builder.Services.AddSingleton<ITodayProvider, SystemTodayProvider>();
builder.Services.AddSingleton<IListingScoringService, ListingScoringService>();
builder.Services.AddScoped<IListingSearchService, ListingSearchService>();

// React development server runs on a different origin.
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDevelopment", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid search request",
            Detail = "One or more search parameters are invalid."
        };

        return new BadRequestObjectResult(problemDetails);
    };
});


var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("ReactDevelopment");
app.MapControllers();

app.Run();

public partial class Program;
