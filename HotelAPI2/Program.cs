using HotelAPI2.Domain;
using HotelAPI2.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<RoomRepository>();
builder.Services.AddScoped<ClientRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ReservationRepository>();
builder.Services.AddScoped<ReservationConfigurationRepository>();
builder.Services.AddDbContext<HotelContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("StoreConnection")));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters()
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["Jwt:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),


	};
});
builder.Services.AddCors(options =>
{
	options.AddPolicy(MyAllowSpecificOrigins,
						  policy =>
						  {
							  policy.WithOrigins("http://localhost:4200", "https://v2.realtravelservices.com")
												  .AllowAnyHeader()
												  .AllowAnyMethod();
						  });
});
var app = builder.Build();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
	ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
	app.UseHttpsRedirection();
}
app.UseAuthentication();
/*
app.UseCors(x => x
			.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader());
*/

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
