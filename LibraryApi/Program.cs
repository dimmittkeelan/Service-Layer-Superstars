using LibraryApi.Data;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Repositories;
using LibraryApi.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IBorrowRecordRepository, BorrowRecordRepository>();

// Register services
builder.Services.AddScoped<IBookService, BookService>();
/* TODO */
//builder.Services.AddScoped<IMemberService, MemberService>();
//builder.Services.AddScoped<IBorrowRecordService, BorrowRecordService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();


