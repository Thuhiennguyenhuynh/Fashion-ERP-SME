//using System.Text;
//using FluentValidation;
//using FluentValidation.AspNetCore;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using CloudinaryDotNet;
//using FashionERP.API.Middleware;
//using FashionERP.Application.Mappings;
//using FashionERP.Infrastructure;          // <-- AddInfrastructure()
//using FashionERP.Infrastructure.Auth;
//using FashionERP.Infrastructure.Data;
//using FashionERP.Infrastructure.Data.Seeders;

//var builder = WebApplication.CreateBuilder(args);

//// 1. DATABASE
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
//        sql => sql.EnableRetryOnFailure(3)));

//// 2. JWT
//var jwtSection = builder.Configuration.GetSection("Jwt");
//builder.Services.Configure<JwtSettings>(jwtSection);
//var jwtSettings = jwtSection.Get<JwtSettings>()!;

//builder.Services.AddAuthentication(opt =>
//{
//    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(opt =>
//{
//    opt.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = jwtSettings.Issuer,
//        ValidAudience = jwtSettings.Audience,
//        IssuerSigningKey = new SymmetricSecurityKey(
//                                       Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
//        ClockSkew = TimeSpan.Zero
//    };
//    opt.Events = new JwtBearerEvents
//    {
//        OnChallenge = ctx =>
//        {
//            ctx.HandleResponse();
//            ctx.Response.StatusCode = 401;
//            ctx.Response.ContentType = "application/json";
//            return ctx.Response.WriteAsync(
//                "{\"success\":false,\"message\":\"Bạn chưa đăng nhập hoặc phiên đã hết hạn\"}");
//        },
//        OnForbidden = ctx =>
//        {
//            ctx.Response.StatusCode = 403;
//            ctx.Response.ContentType = "application/json";
//            return ctx.Response.WriteAsync(
//                "{\"success\":false,\"message\":\"Bạn không có quyền thực hiện thao tác này\"}");
//        }
//    };
//});
//builder.Services.AddAuthorization();

//// 3. CLOUDINARY (singleton)
//var cloud = builder.Configuration.GetSection("Cloudinary");
//var cloudinary = new Cloudinary(new Account(cloud["CloudName"], cloud["ApiKey"], cloud["ApiSecret"]));
//cloudinary.Api.Secure = true;
//builder.Services.AddSingleton(cloudinary);

//// 4. FLUENT VALIDATION
//builder.Services.AddFluentValidationAutoValidation();
//builder.Services.AddValidatorsFromAssemblyContaining<
//    FashionERP.Application.Validators.Auth.LoginRequestValidator>();

//// 5. AUTOMAPPER
//builder.Services.AddAutoMapper(typeof(MappingProfile));

//// 6. ALL INFRASTRUCTURE SERVICES (1 dòng thay thế tất cả AddScoped cũ)
//builder.Services.AddInfrastructure();

//// 7. CONTROLLERS
//builder.Services.AddControllers()
//    .AddJsonOptions(opt =>
//        opt.JsonSerializerOptions.Converters.Add(
//            new System.Text.Json.Serialization.JsonStringEnumConverter()));

//// 8. SWAGGER
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fashion ERP API", Version = "v1" });
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Description = "Nhập: Bearer {access_token}",
//        Name = "Authorization",
//        In = ParameterLocation.Header,
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "Bearer"
//    });
//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {{
//        new OpenApiSecurityScheme
//        {
//            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
//        },
//        Array.Empty<string>()
//    }});
//});

//// 9. CORS
//builder.Services.AddCors(opt => opt.AddPolicy("AllowFrontend", p =>
//    p.WithOrigins("http://localhost:5173", "http://localhost:3000")
//     .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

//// BUILD
//var app = builder.Build();

//// MIDDLEWARE PIPELINE
//app.UseMiddleware<GlobalExceptionMiddleware>();

//if (app.Environment.IsDevelopment())
//{

//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//    {
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fashion ERP API v1");
//        //c.RoutePrefix = string.Empty;
//    });
//}

//app.UseHttpsRedirection();
//app.UseCors("AllowFrontend");
//app.UseAuthentication();
//app.UseAuthorization();
//app.MapControllers();
//app.Run();

using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using CloudinaryDotNet;
using FashionERP.API.Middleware;
using FashionERP.Application.Mappings;
using FashionERP.Infrastructure;
using FashionERP.Infrastructure.Auth;
using FashionERP.Infrastructure.Data;
using FashionERP.Infrastructure.Data.Seeders;

using FashionERP.Application.Interfaces;
using FashionERP.Infrastructure.AIClient;
using FashionERP.Infrastructure.Services;


var builder = WebApplication.CreateBuilder(args);

// ======================
// 1. DATABASE
// ======================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(3)
    )
);
// User
builder.Services.AddScoped<IUserService, UserService>();

// ======================
// 2. JWT CONFIG
// ======================
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSection);

var jwtSettings = jwtSection.Get<JwtSettings>()
    ?? throw new Exception("JWT settings is missing in appsettings.json");

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
        ),

        ClockSkew = TimeSpan.Zero
    };

    opt.Events = new JwtBearerEvents
    {
        OnChallenge = ctx =>
        {
            ctx.HandleResponse();
            ctx.Response.StatusCode = 401;
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync(
                "{\"success\":false,\"message\":\"Bạn chưa đăng nhập hoặc phiên đã hết hạn\"}"
            );
        },
        OnForbidden = ctx =>
        {
            ctx.Response.StatusCode = 403;
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync(
                "{\"success\":false,\"message\":\"Bạn không có quyền thực hiện thao tác này\"}"
            );
        }
    };
});

builder.Services.AddAuthorization();

// ======================
// 3. CLOUDINARY
// ======================
var cloud = builder.Configuration.GetSection("Cloudinary");

var cloudinary = new Cloudinary(new Account(
    cloud["CloudName"],
    cloud["ApiKey"],
    cloud["ApiSecret"]
));

cloudinary.Api.Secure = true;

builder.Services.AddSingleton(cloudinary);

// ======================
// 4. FLUENT VALIDATION
// ======================
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<
    FashionERP.Application.Validators.Auth.LoginRequestValidator>();

// ======================
// 5. AUTOMAPPER
// ======================
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddHttpContextAccessor(); // ← thêm dòng này

// ======================
// 6. INFRASTRUCTURE
// ======================
builder.Services.AddInfrastructure();

// ======================
// 7. CONTROLLERS
// ======================
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
        opt.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        )
    );

// ======================
// 8. SWAGGER
// ======================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fashion ERP API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập: Bearer {access_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ======================
// 9. CORS
// ======================
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


    // 10.Đăng ký HttpClient có tên "AiService" để gọi sang Python FastAPI
builder.Services.AddHttpClient("AiService", client =>
{
    var baseUrl = builder.Configuration["AiService:BaseUrl"]
        ?? throw new InvalidOperationException("Thiếu config AiService:BaseUrl trong appsettings.json");
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

    // 11. Đăng ký client gọi HTTP thuần
    builder.Services.AddScoped<IAIServiceClient, AIServiceClient>();

    // 12. Đăng ký tầng nghiệp vụ AI (build context từ DB + gọi client + ghi log)
    builder.Services.AddScoped<IAIService, AIService>();





// ======================
// BUILD APP
// ======================
var app = builder.Build();

// ======================
// GLOBAL MIDDLEWARE
// ======================
app.UseMiddleware<GlobalExceptionMiddleware>();

// ======================
// DEV ONLY CONFIG
// ======================
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();
    await DatabaseSeeder.SeedAllAsync(db);
    var allUsers = db.Users.ToList();
    foreach (var user in allUsers)
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Fashion@2025", 12);
    }
    await db.SaveChangesAsync();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fashion ERP API v1");
    });
}

// ======================
// PIPELINE
// ======================
app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ======================
// RUN
// ======================
app.Run();


