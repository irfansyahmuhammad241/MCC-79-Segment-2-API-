using API.Contracts;
using API.Data;
using API.Repositories;
using API.Services;
using API.Utilities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BookingDbContext>(options => options.UseSqlServer(connectionString));

// Add Repository to the container
builder.Services.AddScoped<IUniversityRepository, UniversityRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEducationRepository, EducationRepository>();
builder.Services.AddScoped<IAccountRoleRepository, AccountRoleRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ITokenHandler, TokenHandler>();



//Add Service to the container
builder.Services.AddScoped<UniversityService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<AccountRoleService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<EducationService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<BookingService>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();