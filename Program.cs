using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using SneakersPlanet.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDb");
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<MongoDbContext>(serviceProvider =>
{
    var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["MongoDbSettings:DatabaseName"];
    return new MongoDbContext(mongoClient, databaseName);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
