using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApplicationAuth.Api.DataBase;
using WebApplicationAuth.Api.DataBase.Models;

namespace WebApplicationAuth.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = global::Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure EF Core AppDbContext and MS SQL Server.
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add MS Identity.
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>() // Arguments are AddIdentity<TUser, TRole>(null).
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Add MS Auth.
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                // JWT bearer authentication performs authentication by extracting and validating a JWT token from the Authorization request header.
            .AddJwtBearer(options => // Add JWT Bearer (JWT validating mechanism).
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; // Disabled for development purposes to allow HTTP requests.
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JWT:Secret"]!)),
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                };
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseHsts(); // Adds middleware for using HSTS, which adds the Strict-Transport-Security header.
            }

            // When 'Configure for HTTPS' was enabled during project creation.
            app.UseHttpsRedirection(); // Adds middleware for redirecting HTTP Requests to HTTPS.

            app.UseRouting();

            // Authentication.
            app.UseAuthentication();
            // Authorization.
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
