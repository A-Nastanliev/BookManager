using BusinessLayer.Repositories;
using DataLayer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServiceLayer.Services;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"]);
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,   
        ClockSkew = TimeSpan.Zero     
    };
});

builder.Services.AddAuthorization();
builder.Services.AddDbContext<BookManagerContext>(options =>
    options.UseMySQL(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();
builder.Services.AddScoped<AuthorRepository>();
builder.Services.AddScoped<BookCommentRepository>();
builder.Services.AddScoped<BookRatingRepository>();
builder.Services.AddScoped<BookRepository>();
builder.Services.AddScoped<BookRequestRepository>();
builder.Services.AddScoped<GenreRepository>();
builder.Services.AddScoped<PublisherRepository>();
builder.Services.AddScoped<ReadingLogRepository>();
builder.Services.AddScoped<UserBookRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserRestrictionRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
