using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using bankassessment.Controllers;
using Microsoft.Extensions.Logging;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.SimpleNotificationService.Model;
using bankassessment.Repository;

namespace Tests;

public class UnitTest1
{
    public class BankAccountControllerTests
    {
        private readonly Mock<IBankAccountRepository> _mockRepository;
        private readonly Mock<IAmazonSimpleNotificationService> _mockSnsClient;
        private readonly Mock<ILogger<BankAccountController>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly BankAccountController _controller;

        public BankAccountControllerTests()
        {
            _mockRepository = new Mock<IBankAccountRepository>();
            _mockSnsClient = new Mock<IAmazonSimpleNotificationService>();
            _mockLogger = new Mock<ILogger<BankAccountController>>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(c => c["AWS:SNSTopicArn"]).Returns("arn:aws:sns:us-east-1:123456789012:MyTopic");

            _controller = new BankAccountController(_mockRepository.Object, _mockConfiguration.Object, _mockSnsClient.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Withdraw_ShouldReturnBadRequest_WhenAmountIsZeroOrNegative()
        {
            var result = await _controller.Withdraw(123456, 0);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Amount must be greater than 0", badRequestResult.Value);
        }

        [Fact]
        public async Task Withdraw_ShouldReturnBadRequest_WhenAccountNotFound()
        {
            var accountId = 123456;
            var amount = 100;
            _mockRepository.Setup(r => r.GetAccountAsync(accountId.ToString())).ReturnsAsync((Document)null);

            var result = await _controller.Withdraw(accountId, amount);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Account not found", badRequestResult.Value);
        }

        [Fact]
        public async Task Withdraw_ShouldReturnBadRequest_WhenInsufficientFunds()
        {
            var accountId = 123456;
            var amount = 200;
            var document = new Document();
            document["Balance"] = 100;

            _mockRepository.Setup(r => r.GetAccountAsync(accountId.ToString())).ReturnsAsync(document);

            var result = await _controller.Withdraw(accountId, amount);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Insufficient funds", badRequestResult.Value);
        }

        [Fact]
        public async Task Withdraw_ShouldReturnOk_WhenWithdrawalIsSuccessful()
        {
            var accountId = 123456;
            var amount = 100;
            var document = new Document();
            document["Balance"] = 200;

            _mockRepository.Setup(r => r.GetAccountAsync(accountId.ToString())).ReturnsAsync(document);
            _mockSnsClient.Setup(s => s.PublishAsync(It.IsAny<PublishRequest>(), default)).ReturnsAsync(new PublishResponse { MessageId = "12345" });

            var result = await _controller.Withdraw(accountId, amount);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Withdrawal successful", okResult.Value);
        }

        [Fact]
        public async Task Withdraw_ShouldReturnInternalServerError_WhenSnsPublishFails()
        {
            var accountId = 123456;
            var amount = 100;
            var document = new Document();
            document["Balance"] = 200;

            _mockRepository.Setup(r => r.GetAccountAsync(accountId.ToString())).ReturnsAsync(document);
            _mockSnsClient.Setup(s => s.PublishAsync(It.IsAny<PublishRequest>(), default)).ThrowsAsync(new Exception("SNS publish failed"));

            var result = await _controller.Withdraw(accountId, amount);

            var objectResult = Assert.IsType<ObjectResult>(result); // Expecting ObjectResult here
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal server error", objectResult.Value);
        }

    }
}
