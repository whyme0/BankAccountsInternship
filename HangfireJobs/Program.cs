using Hangfire;
using Hangfire.PostgreSql;
using HangfireJobs.Jobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfire(c1 => {
    c1.UsePostgreSqlStorage(c2 =>
    {
        c2.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
});
builder.Services.AddHangfireServer();
builder.Services.AddScoped<IJob, AccrueInterest>();

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
    job => job.AccrueInterestAsync(),
    Cron.Daily);

app.Run();
