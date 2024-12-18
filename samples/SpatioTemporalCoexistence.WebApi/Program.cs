using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .CreateLogger();
//builder.Logging.ClearProviders();
builder.Host.UseSerilog(Log.Logger, dispose: true);
//解决Multipart body length limit 134217728 exceeded
builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
});

//配置跨域
builder.Services.AddCors(cor =>
{
    var cors = builder.Configuration.GetSection("CorsUrls").GetChildren().Select(p => p.Value);
    cor.AddPolicy("Cors", policy =>
    {
        policy.WithOrigins(cors.ToArray())//设置允许的请求头
        .WithExposedHeaders("x-custom-header")//设置公开的响应头
        .AllowAnyHeader()//允许所有请求头
        .AllowAnyMethod()//允许任何方法
        .AllowCredentials()//允许跨源凭据----服务器必须允许凭据
        .SetIsOriginAllowed(_ => true);
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors("Cors");

app.Run();