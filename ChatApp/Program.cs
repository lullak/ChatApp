using ChatApp.Core.Interfaces.Repos;
using ChatApp.Core.Interfaces.Services;
using ChatApp.Core.Services;
using ChatApp.Data.Contexts;
using ChatApp.Data.Repos;
using ChatApp.Hubs;
using ChatApp.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

try
{
    SeriLogHelper.Init();
    var builder = WebApplication.CreateBuilder(args);


    builder.Host.UseSerilog();


    builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    });

    //EF
    builder.Services.AddDbContext<ChatDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    //DI
    builder.Services.AddScoped<IChatRepo, ChatRepo>();
    builder.Services.AddSingleton<ITokenService, TokenService>();
    builder.Services.AddSingleton<IAesKeyService, AesKeyService>();

    //SignalR
    builder.Services.AddSignalR();

    //JWT Auth
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               ValidIssuer = builder.Configuration["Jwt:Issuer"],
               ValidAudience = builder.Configuration["Jwt:Audience"],
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
           };
           options.Events = new JwtBearerEvents
           {
               OnMessageReceived = context =>
               {
                   if (context.Request.Cookies.TryGetValue("token", out var token))
                   {
                       context.Token = token;
                   }
                   return Task.CompletedTask;
               },
               OnChallenge = context =>
               {
                   context.HandleResponse();
                   context.Response.Redirect("/Auth");
                   return Task.CompletedTask;
               }
           };
       });

    builder.Services.AddAuthorization();

    //Kestrel
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(7061, listenOptions =>
        {
            listenOptions.UseHttps();
        });
    });
    var app = builder.Build();

    app.UseHsts();

    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    //SignalR
    app.MapHub<ChatHub>("/chathub");

    app.Run();

}
catch (Exception e)
{
    Log.Fatal(e, "UNEXPECTED SERVER TERMINATION BRUMBRUM");
    throw;
}
finally
{
    Log.CloseAndFlush();
}