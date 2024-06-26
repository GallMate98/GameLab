global using GameLab.Services.Email;
using GameLab.Data;
using GameLab.Hubs;
using GameLab.Models;
using GameLab.Repositories;
using GameLab.Services.DataService;
using GameLab.Services.GameLobbyAssignment;
using GameLab.Services.GameScoreCalculator;
using GameLab.Services.LobbyAssignment;
using GameLab.Services.NineMensMorrisService.cs;
using GameLab.Services.TicTacToe;
using GameLab.Services.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<DataContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:3000").AllowCredentials();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<GameScoreRepository>();
builder.Services.AddScoped<IGameScoreCalculatorService, GameScoreCalculatorService>();
builder.Services.AddSingleton<ILobbyAssignmentService, LobbyAssignmentService>();
builder.Services.AddSingleton<IGameAssignmentService, GameAssignmentService>();
builder.Services.AddSingleton<SharedDb>();

builder.Services.AddSingleton<TicTacToeService>();
builder.Services.AddSingleton<NineMensMorrisService>();

builder.Services.AddIdentityCore<User>()
    .AddRoles<IdentityRole>()
    .AddTokenProvider<DataProtectorTokenProvider<User>>("Data")
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.Password.RequiredLength = 6;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        //ValidateAudience = true,
        //ValidateIssuer = true,
        ValidateAudience = false,
        ValidateIssuer = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        //ValidIssuer = builder.Configuration["Jwt:Issure"],
       // ValidAudience = builder.Configuration["Jwt:Audience"],
       
        IssuerSigningKey = new SymmetricSecurityKey(
         Encoding.UTF8.GetBytes
        (builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<LobbyHub>("/lobbyhub");
app.MapHub<GameHub>("/gamehub");
app.MapHub<NineMensMorrisHub>("/ninemenshub");


app.MapControllers();

app.Run();
