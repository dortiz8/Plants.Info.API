using Plants.info.API.Data;
using Plants.info.API.Data.Contexts;
using Plants.info.API.Data.Repository;
using Plants.info.API.Security.Data.Services.Extensions;
using Plants.info.API.Data.Services.Extensions;
using Plants.info.API.Persistence.Data.Services.Extensions; 
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Plants.info.API.Data.Services.JwtFeatures;
using Plants.info.API.Data.Services;
using Plants.info.API.Data.Services.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("Logs/plantsInfo.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger(); 

var builder = WebApplication.CreateBuilder(args); // Used to host the application 

builder.Host.UseSerilog(); // Registers the new logger instead of the ASP.NET Core implementation. 

var connectionString = builder.Configuration.GetConnectionString("UserContextDb");
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

//builder.Services.AddTransient<UserSeeder>(); // new instance is created per request
//builder.Services.AddScoped<IUserRepository, UserRepository>(); // one instance per request
//builder.Services.AddScoped<IMenusRepository, MenusRepository>(); // one instance per request
builder.Services.AddBusinessInfrastructure();
builder.Services.AddSingleton<FileExtensionContentTypeProvider>(); 
builder.Services.AddDbContext<UserContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddSecurityInfrastructure();
builder.Services.AddPersistenceInfrastructure(); 

// Authentication services 
builder.Services.AddAuthentication((x) =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
   // x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true; 
    options.TokenValidationParameters = new TokenValidationParameters 
    {
        ValidIssuer = builder.Configuration["Authentication:Issuer"],
        ValidAudience = builder.Configuration["Authentication:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Authentication:SecretForKey"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
    };
});

// Authentication policies
//builder.Services.AddAuthorization(options =>
//{
//    //options.AddPolicy("IdsMustMatch", policy =>
//    //{
//    //    policy.RequireAuthenticatedUser();
//    //    policy.RequireClaim("userId", "1");
//    //});
//    //options.AddPolicy("RoleMustBeAdmin", policy =>
//    //{
//    //    policy.RequireAuthenticatedUser();
//    //    policy.RequireClaim("access_role", "9");
//    //}); 
//});

builder.Services.AddAuthorization(); 

#if DEBUG
    //builder.Services.AddScoped<IPlantsRepository, PlantsRepository>();
#else
    builder.Services.AddScoped<IPlantsRepository, PlantsRepository>();
#endif
builder.Services.AddCors(option =>
{
    option.AddPolicy("CorsPolicy",
        builder => builder.WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        ); 
}); 

builder.Services.AddControllers( options => 
{
    options.ReturnHttpNotAcceptable = true; // Returns message to the client that the content type expected is not supported with status code 406 Not Acceptable. 
}).AddNewtonsoftJson() // Replaces the default JSON input and output formatters with Json.NET
    .AddXmlDataContractSerializerFormatters(); // Adds support for returning XML
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddScoped<IJwtHandler, JwtHandler>(); 
// Once all services are built and configured, then the web app can be built. 
var app = builder.Build();

if (args.Length == 1 && args[0].ToLower() == "/seed")
    //if (args.Length == 0)
        RunSeeding(app);

void RunSeeding(IHost app)
{
    var scopedFactory = app.Services.GetService<IServiceScopeFactory>();
    using (var scope = scopedFactory.CreateScope()) // Using to close the scope once we are done
    {
        var seeder = scope.ServiceProvider.GetService<UserSeeder>();
        //seeder.Unseed(); // We can possibly use this to clear out seeded data
        seeder.SeedSampleData(builder.Configuration["Paths:JsonSamplePlants"]);
    }
}

// Configure the HTTP request pipeline.
// Order matters when using methods in the pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Swagger specifications
    app.UseSwaggerUI();// builds a UI
}

// Enable enpoint routing 
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");

app.UseAuthentication(); 
app.UseAuthorization();


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
}); 
app.MapControllers();

app.Run();
