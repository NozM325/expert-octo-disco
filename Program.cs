using Amazon;
using Amazon.DynamoDBv2;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.DependencyInjection;
using bankassessment.Repository;

var builder = WebApplication.CreateBuilder(args);

// Register the BankAccountRepository as a scoped dependency
builder.Services.AddScoped<IBankAccountRepository, BankAccountRepository>();

// Add controller support
builder.Services.AddControllers();

// Register AWS SNS client as a singleton
builder.Services.AddSingleton<IAmazonSimpleNotificationService>(_ =>
    new AmazonSimpleNotificationServiceClient(RegionEndpoint.USEast1));

// Register AWS DynamoDB client as a singleton
builder.Services.AddSingleton<IAmazonDynamoDB>(_ =>
    new AmazonDynamoDBClient(RegionEndpoint.USEast1));

// Build the application
var app = builder.Build();

// Enable routing for HTTP endpoints
app.UseRouting();

// Map controller endpoints
app.MapControllers();
app.Run();

