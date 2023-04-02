using HotelAPI2.Repositories;

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
builder.Services.AddCors(options =>
{
	options.AddPolicy(MyAllowSpecificOrigins,
						  policy =>
						  {
							  policy.WithOrigins("http://localhost:4200")
												  .AllowAnyHeader()
												  .AllowAnyMethod();
						  });
});
var app = builder.Build();

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

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
