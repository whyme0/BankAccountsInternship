using Api.Data;
using Hangfire;
using Hangfire.PostgreSql;
using HangfireJobs.Jobs;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

var factory = new ConnectionFactory()
{
    HostName = "rabbitmq",
    UserName = "admin",
    Password = "admin"
};

IConnection rabbitConnection = await factory.CreateConnectionAsync();

builder.Services.AddSingleton<IConnection>(rabbitConnection);

builder.Services.AddDbContext<IAppDbContext, AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfire(c1 => {
    c1.UsePostgreSqlStorage(c2 =>
    {
        c2.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
});
builder.Services.AddHangfireServer();
builder.Services.AddScoped<IJob, AccrueInterest>();
builder.Services.AddScoped<IJob, OutboxDispatcherJob>();

builder.Services.AddLogging();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = []
    });
}

app.MapGet("/", async context =>
{
    context.Response.Redirect("/hangfire");
    await context.Response.CompleteAsync();
});

RecurringJob.AddOrUpdate<AccrueInterest>(
    "AccrueInterestJob",
    job => job.Execute(CancellationToken.None),
    Cron.Daily);
RecurringJob.AddOrUpdate<OutboxDispatcherJob>(
    "OutboxDispatcherJob",
    job => job.Execute(CancellationToken.None),
    Cron.Hourly);

app.Run();
