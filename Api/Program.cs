using Api.Data;
using Api.Data.Initializer;
using Api.Exceptions;
using Api.Filters;
using Api.PipelineBehaviors;
using Api.Presentation;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "BankAccountsAuthApi",

            ValidateAudience = true,
            ValidAudience = "BankAccountsApi",

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey("this-is-should-be-pretty-secure-secret-key"u8.ToArray())
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var result = new MbResult
                {
                    MbError = [new MbError
                    {
                        PropertyName = "RESOURCE",
                        ErrorMessage = "Unauthorized"
                    }],
                    StatusCode = StatusCodes.Status401Unauthorized
                };

                await context.Response.WriteAsJsonAsync(result);
            }
        };
    });

builder.Services
    .AddControllers(o =>
    {
        o.Filters.Add<MbResultFilter>();
        o.Filters.Add(new AuthorizeFilter());
    })
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    o.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Bank accounts API",
        Description = "API сервиса банка по работе со счетами"
    });

    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Token"
    });

    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(defaultConnectionString));
builder.Services.AddScoped<IAppDbContext, AppDbContext>();

builder.Services.AddMediatR(c => 
    c.RegisterServicesFromAssembly(typeof(Program).Assembly));

ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });

    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/swagger");
            return;
        }
        await next();
    });
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
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
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
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
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new MbResult
                {
                    MbError = [new MbError
                    {
                        PropertyName = "RESOURCE",
                        ErrorMessage = "Not found"
                    }],
                    StatusCode = StatusCodes.Status404NotFound
                });
                break;
            case { } e:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new MbResult
                {
                    MbError = [new MbError
                    {
                        PropertyName = "RESOURCE",
                        ErrorMessage = e.Message
                    }],
                    StatusCode = StatusCodes.Status500InternalServerError
                });
                break;
        }
    });
});

app.Run();