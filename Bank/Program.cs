using Bank.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.Globalization;

string[] name = ["Goal", "Transaction", "User", "Auth"];


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<Bank.Context.UserContext>();
builder.Services.AddDbContext<Bank.Context.TransactionContext>();
builder.Services.AddDbContext<Bank.Context.GoalContext>();


builder.Services.AddRazorPages();
builder.Services.AddMvc(option => option.EnableEndpointRouting = false);

builder.Services.AddSwaggerGen(c =>
{
    for (int i = 0; i < name.Length; i++)
    {
        c.SwaggerDoc($"v{i + 1}", new OpenApiInfo
        {
            Version = $"v{i + 1}",
            Title = name[i]
        });
    }
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    for (int i = 0; i < name.Length; i++)
    {
        c.SwaggerEndpoint($"/swagger/v{i + 1}/swagger.json", name[i]);
    }
    c.RoutePrefix = string.Empty;
});

app.MapControllers();

app.Run();