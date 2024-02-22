using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Filebin.Common.Util;
using Filebin.AuthServer.Data;
using Filebin.Common.Exceptions;
using Filebin.Common.Commands;
using System.Reflection;

namespace Filebin.AuthServer;

public class Program {
    private static async Task Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        var services = builder.Services;

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.ConfigureSwaggerJwt();

        services.AddControllers();


        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetNpgsqlConnectionString("auth_server")));

        services.AddDefaultIdentity<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.Configure<IdentityOptions>(options => {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;
        });

        services.ConfigureUtilServices(builder.Configuration);
        services.ConfigureExceptions();
        services.AddCommandsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddServices();

        var app = builder.Build();

        if (app.Environment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.AddUtilLayers();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        using (var scope = app.Services.CreateScope()) {
            await scope.ServiceProvider.ConfigureRolesAsync();
        }

        app.Run();
    }
}