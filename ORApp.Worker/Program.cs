using Microsoft.EntityFrameworkCore;
using ORApp.Data.Context;
using ORApp.Worker;
using System;



var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();


builder.Services.AddHttpClient("RapidApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["RapidApi:BaseUrl"]);
    client.DefaultRequestHeaders.Add("x-rapidapi-key", builder.Configuration["RapidApi:Key"]);
    client.DefaultRequestHeaders.Add("x-rapidapi-host", builder.Configuration["RapidApi:Host"]);
});


builder.Services.AddDbContext<ORDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var host = builder.Build();
host.Run();
