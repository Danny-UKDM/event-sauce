using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using EventSauceApi;
using EventSauceApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DefaultOptions>(builder.Configuration.GetSection("EventSauce"));

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions("AWS"));
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
builder.Services.AddSingleton<IDynamoOperationConfig, DynamoOperationConfig>();

builder.Services.AddSingleton<IEventGetter, EventGetter>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program { }
