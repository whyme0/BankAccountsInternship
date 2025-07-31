using Api.Data;
using Api.Data.Initializer;
using Api.PipelineBehaviors;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Api.Exceptions;
using Api.Filters;
using Api.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(o =>
    {
        o.Filters.Add<MbResultFilter>();
    })
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=BankDb.db"));
builder.Services.AddScoped<IAppDbContext, AppDbContext>();

builder.Services.AddMediatR(c => 
    c.RegisterServicesFromAssembly(typeof(Program).Assembly));

ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    await DbSeeder.SeedAsync(dbContext);
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

        switch (exceptionHandlerPathFeature?.Error)
        {
            case ValidationException validationException:
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new MbResult
                {
                    MbError = validationException.Errors.Select(e => new MbError
                    {
                        PropertyName = e.PropertyName,
                        ErrorMessage = e.ErrorMessage
                    }).ToList(),
                    StatusCode = StatusCodes.Status400BadRequest
                });
                break;
            case NotFoundException:
                context.Response.StatusCode = 404;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new MbResult
                {
                    MbError = [new MbError
                    {
                        PropertyName = "resource",
                        ErrorMessage = "Not found"
                    }],
                    StatusCode = StatusCodes.Status404NotFound
                });
                break;
        }
    });
});

app.Run();