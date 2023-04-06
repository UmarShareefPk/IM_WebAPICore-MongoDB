
using IM.Hubs;
using IM_DataAccess.DataService;
using IM_WebAPICore_MongoDB.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(jsonOptions =>
{
    jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;
})
 .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

builder.Services.AddSignalR();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options => {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = false,
                       ValidateIssuerSigningKey = true,
                       ValidIssuer = "IM Core with Mongo DB",
                       ValidAudience = "all",
                       ClockSkew = TimeSpan.Zero,
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSecret"]))
                   };
                   options.Events = new JwtBearerEvents
                   {
                       OnAuthenticationFailed = async ctx =>
                       {
                           var putBreakpointHere = true;
                           var exceptionMessage = ctx.Exception;
                       },
                   };
               });

builder.Services.AddMemoryCache();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientPermission", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            //.AllowAnyOrigin()
            .WithOrigins(
                "https://localhost:44338",
                "http://localhost:3000",
                "http://localhost:4200",
                "https://lively-bush-0d9b77d10.1.azurestaticapps.net",
                "http://localhost/ImAngular",
                "https://calm-mud-02aada210.1.azurestaticapps.net",
                "https://localhost:7135",
                "https://salmon-bay-0ee5f3310.1.azurestaticapps.net",
                "https://immvc6.azurewebsites.net",
                "https://incidentbyid.azurewebsites.net",
                "https://green-coast-003a53010.1.azurestaticapps.net"
                )
            .AllowCredentials();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IIncidentService, IncidentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IJWT, JWT>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseCors("ClientPermission");


app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
