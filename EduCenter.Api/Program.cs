using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using EduCenter.Api.Authentication;
using EduCenter.Api.Errors;
using EduCenter.Api.Middleware;
using EduCenter.Api.Swagger;
using EduCenter.Business.Abstractions.Repositories;
using EduCenter.Business.Models;
using EduCenter.Business.Services;
using EduCenter.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["traceId"] = traceId;
    };
});
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Введите access token, полученный от POST /api/auth/login."
    });
    options.OperationFilter<AuthorizeOperationFilter>();
    options.OperationFilter<CartPriceOperationFilter>();
});

builder.Services.AddScoped<ICartPricingService, CartPricingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
builder.Services.AddScoped<ILeadService, LeadService>();

builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<IUserRepository>(
    services => services.GetRequiredService<UserRepository>());
builder.Services.AddSingleton<IUserCredentialRepository>(
    services => services.GetRequiredService<UserRepository>());
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddSingleton<IPasswordHasher<UserCredential>, PasswordHasher<UserCredential>>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddSingleton<IAccessTokenService, JwtAccessTokenService>();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
jwtOptions.Validate();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtOptions.GetSigningKeyBytes()),
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            NameClaimType = JwtRegisteredClaimNames.Sub,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
