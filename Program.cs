using Amazon;
using Amazon.DynamoDBv2;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.DependencyInjection;
using bankassessment.Repository;

var builder = WebApplication.CreateBuilder(args);

// Register your repository with the DI container
builder.Services.AddScoped<IBankAccountRepository, BankAccountRepository>();

// Add controllers
builder.Services.AddControllers();

// Add other necessary services (e.g., AWS services)
builder.Services.AddSingleton<IAmazonSimpleNotificationService>(_ =>
    new AmazonSimpleNotificationServiceClient(RegionEndpoint.USEast1));

builder.Services.AddSingleton<IAmazonDynamoDB>(_ =>
    new AmazonDynamoDBClient(RegionEndpoint.USEast1));

var app = builder.Build();
app.UseRouting();
app.MapControllers();
app.Run();

