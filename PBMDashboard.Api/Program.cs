using PBMDashboard.Api.Data;
using PBMDashboard.Api.Services;
using Microsoft.EntityFrameworkCore;

SQLitePCL.Batteries_V2.Init();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<PbmDashboardDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("PbmDashboardDatabase") ?? "Data Source=pbm-dashboard.db"));
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<PbmDashboardService>();
builder.Services.AddScoped<PbmDemoDataSeeder>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PbmDashboardDbContext>();
    dbContext.Database.EnsureCreated();
    var seeder = scope.ServiceProvider.GetRequiredService<PbmDemoDataSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
