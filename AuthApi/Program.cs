using AuthApi;
using AuthApi.Managers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

app.MapPost("/login", (CreateJwtTokenDto dto) => Results.Created(".", new
{
    Token = JwtTokenManager.CreateJwtToken(dto)
}));

app.MapGet("/", () => "Ok");

app.UseHttpsRedirection();


app.Run();