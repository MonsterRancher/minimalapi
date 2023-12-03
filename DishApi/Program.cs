using System.Net;
using System.Security.Claims;
using AutoMapper;
using DishesAPI.DbContexts;
using DishesAPI.Entities;
using DishesAPI.Extensions;
using DishesAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//restister the DBContext on the container, getting the 
//connection string from appSettings
builder.Services.AddDbContext<DishesDbContext>(o => o.UseSqlite(
    builder.Configuration["ConnectionStrings:DishesDBConnectionString"]));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddProblemDetails();

builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdminFromBelgium",policy => policy.RequireRole("admin")
                                                         .RequireClaim("country","Belgium"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("TokenAuthNZ",
        new()
        {
            Name = "Authorization",
            Description = "Token-based authentication and authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            In = ParameterLocation.Header
        });
    options.AddSecurityRequirement(new()
    {
        { 
            new()
            { 
                Reference = new OpenApiReference{ 
                    Type = ReferenceType.SecurityScheme,
                    Id = "TokenAuthNZ"
                }
            }, new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler();
    //app.UseExceptionHandler(configureApplicationBuilder => {
    //    configureApplicationBuilder.Run(
    //        async context =>
    //        {
    //            context.Response.StatusCode = (int)
    //               HttpStatusCode.InternalServerError;
    //            context.Response.ContentType = "text/html";
    //            await context.Response.WriteAsync("An unexpected problem happned.");
    //        }
    //    );
    //});
}


app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();



app.RegisterDishesEndpoints();
app.RegisterIngredientsEndpoints();

app.Run();
