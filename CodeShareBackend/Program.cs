using CodeShareBackend.Controllers;
using CodeShareBackend.Data;
using CodeShareBackend.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin();
            builder.AllowAnyMethod();
            builder.AllowAnyHeader();
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TrelloDBConnection"))
    );


builder.Services.AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

//builder.Services.AddSingleton<CodeSnippetController>();

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddSignalR();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowAllOrigins");

app.MapControllers();

app.MapHub<CodeShareHub>("/codesharehub");

app.MapIdentityApi<User>();

app.Run();

