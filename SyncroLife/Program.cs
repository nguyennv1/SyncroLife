using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using SyncroLife.AIRecommendation;
using SyncroLife.BackgroundServices;
using SyncroLife.Data;
using SyncroLife.Helpers;
using SyncroLife.Interfaces.Repositories;
using SyncroLife.Interfaces.Services;
using SyncroLife.Repositories;
using SyncroLife.Services;
using SyncroLife.Services.BackgroundServices;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is missing");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = "role",
        NameClaimType = "name"
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
            if (identity != null)
            {
                // Duplicate "sub" as ClaimTypes.NameIdentifier so both FindFirst("sub") and FindFirst(ClaimTypes.NameIdentifier) work.
                var subClaim = identity.FindFirst("sub");
                if (subClaim != null && !identity.HasClaim(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier))
                {
                    identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, subClaim.Value));
                }

                // Duplicate "role" as ClaimTypes.Role to support standard Role checks.
                var roleClaim = identity.FindFirst("role");
                if (roleClaim != null && !identity.HasClaim(c => c.Type == System.Security.Claims.ClaimTypes.Role))
                {
                    identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, roleClaim.Value));
                }
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllers();

//Allow Flutter Web app running on any localhost port (and all origins for development).
//Restrict origins before deploying to production.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutterWeb", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)   // allow any localhost port
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtHelper>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();

builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<IGoalService, GoalService>();

builder.Services.AddScoped<IHabitRepository, HabitRepository>();
builder.Services.AddScoped<IHabitService, HabitService>();

builder.Services.AddTransient<JwtHelper>();

builder.Services.AddScoped<IScheduleTypeRepository, ScheduleTypeRepository>();
builder.Services.AddScoped<IScheduleTypeService, ScheduleTypeService>();

builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();

builder.Services.AddScoped<IReminderRepository, ReminderRepository>();
builder.Services.AddScoped<IReminderService, ReminderService>();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddHostedService<ReminderBackgroundService>();
builder.Services.AddHostedService<RecommendationBackgroundService>();

builder.Services.AddScoped<IAiRecommendationRepository, AiRecommendationRepository>();

builder.Services.AddScoped<IRecommendationEngine, GeminiRecommendationEngine>();

builder.Services.AddScoped<IRecommendationService, RecommendationService>();

builder.Services.AddScoped<IMealRepository, MealRepository>();
builder.Services.AddScoped<IUserDietaryPreferenceRepository, UserDietaryPreferenceRepository>();

builder.Services.AddScoped<IUserDeviceTokenRepository, UserDeviceTokenRepository>();

builder.Services.AddScoped<IUserDeviceTokenService, UserDeviceTokenService>();

builder.Services.AddHttpClient();
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));
builder.Services.AddHttpClient<IGeminiService, GeminiService>();

builder.Services.AddScoped<IFoodAnalysisRepository, FoodAnalysisRepository>();

builder.Services.AddScoped<IFoodAnalysisService, FoodAnalysisService>();

builder.Services.AddDbContext<SyncroLifeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Token from API Login."
        };

        document.Components.SecuritySchemes.Add("Bearer", securityScheme);
        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        if (context.Description.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            var methodInfo = controllerActionDescriptor.MethodInfo;

            var methodAuthorize = Attribute.IsDefined(methodInfo, typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute));
            var classAuthorize = methodInfo.DeclaringType != null && Attribute.IsDefined(methodInfo.DeclaringType, typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute));

            if (methodAuthorize || classAuthorize)
            {
                operation.Security ??= new List<OpenApiSecurityRequirement>();

                var schemeRef = new OpenApiSecuritySchemeReference("Bearer");
                var requirement = new OpenApiSecurityRequirement
                {
                    [schemeRef] = new List<string>()
                };

                operation.Security.Add(requirement);
            }
        }

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Seed default subscription plans
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<SyncroLife.Data.SyncroLifeDbContext>();
        
        var freePlanId = Guid.Parse("8a6c40ca-ef52-4dfb-a977-127c1d66d08b");
        var freePlan = await context.SubscriptionPlans.FindAsync(freePlanId);
        if (freePlan == null)
        {
            freePlan = new SyncroLife.Models.SubscriptionPlan
            {
                PlanId = freePlanId,
                PlanName = "Free",
                Description = "Free plan with 3 AI scans per 24 hours",
                Price = 0,
                DurationDays = 36500,
                Features = "3 scans/day",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await context.SubscriptionPlans.AddAsync(freePlan);
        }

        var plusPlanId = Guid.Parse("5b7d12f3-ea11-40ef-bc28-98d01cd59e0a");
        var plusPlan = await context.SubscriptionPlans.FindAsync(plusPlanId);
        if (plusPlan == null)
        {
            plusPlan = new SyncroLife.Models.SubscriptionPlan
            {
                PlanId = plusPlanId,
                PlanName = "Plus",
                Description = "Plus plan with 20 AI scans per 24 hours",
                Price = 50000,
                DurationDays = 30,
                Features = "20 scans/day",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await context.SubscriptionPlans.AddAsync(plusPlan);
        }
        else
        {
            plusPlan.Description = "Plus plan with 20 AI scans per 24 hours";
            plusPlan.Features = "20 scans/day";
            plusPlan.Price = 50000;
            plusPlan.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database subscription plans.");
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Always map API docs so they are accessible in the production environment for demo/testing
app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

// Enable CORS before routing/auth so preflight OPTIONS requests are handled correctly.
app.UseCors("AllowFlutterWeb");

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

        if (error != null)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                Message = error.Error.Message,
                Detail = error.Error.InnerException?.Message
            });
        }
    });
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Redirect root URL to Scalar API docs so users don't see a 404 page
app.MapGet("/", () => Results.Redirect("/scalar/v1"));

app.Run();