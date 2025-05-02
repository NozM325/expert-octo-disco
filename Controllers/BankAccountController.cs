using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2;
using Amazon.SimpleNotificationService;
using bankassessment.Models;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using bankassessment.Repository;

namespace bankassessment.Controllers
{
    [Route("bank")]
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountRepository _repository;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly string _snsTopicArn;
        private readonly ILogger<BankAccountController> _logger;
        
        // Constructor to initialize the controller with necessary dependencies
        // _repository is used for accessing the bank account data
        // _snsClient is used to interact with the AWS SNS service for publishing notifications
        // _snsTopicArn is the ARN of the SNS topic, which is used for publishing withdrawal events
        // _logger is used for logging withdrawl information
        public BankAccountController(IBankAccountRepository repository, IConfiguration configuration, IAmazonSimpleNotificationService snsClient, ILogger<BankAccountController> logger)
        {
            _repository = repository;
            _snsClient = snsClient;
            _snsTopicArn = configuration["AWS:SNSTopicArn"];
            _logger = logger;
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromQuery] long accountId, [FromQuery] decimal amount)
        {
            
            // Retrieve the account data based on the accountId

            var document = await _repository.GetAccountAsync(accountId.ToString());
            var currentBalance = document["Balance"].AsDecimal();

            // Ensure the withdrawal amount is greater than 0
            if (amount <= 0)
            {
                return BadRequest("Amount must be greater than 0");
            }

            _logger.LogInformation("Withdrawing {Amount} from account {AccountId}", amount, accountId);

             // Check if the account exists in the database
             // If account is not found or balance is missing, return bad request
            if (document == null || !document.Contains("Balance"))
                return BadRequest("Account not found");

            // Check if the account has sufficient balance to perform the withdrawal
            // Insufficient fund
            if (currentBalance < amount)
                return BadRequest("Insufficient funds");

            // Update balance
            document["Balance"] = currentBalance - amount;
            await _repository.UpdateAccountAsync(document);

            // Create a Withdrawal object and publish it to SNS to notify other systems of the withdrawal
            // Unique MessageGroupId and MessageDeduplicationId to avoid duplication
            // Publish to SNS
            var withdrawal = new Withdrawal(amount, accountId, "SUCCESSFUL");
            var publishRequest = new PublishRequest
            {
                Message = withdrawal.ToJson(),
                TopicArn = _snsTopicArn,
                MessageGroupId = accountId.ToString(), // Ensures FIFO (First-In-First-Out)
                MessageDeduplicationId = Guid.NewGuid().ToString()
            };

            try
            {
                // Attempt to publish the message to SNS and log any errors if publishing fails
                await _snsClient.PublishAsync(publishRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SNS Publish failed");
                return StatusCode(500, "Internal server error");
            }

            // Audit record for the successful withdrawal transaction
            var audit = new Audit
            {
                AccountId = accountId,
                Amount = amount,
                Status = "SUCCESSFUL",
            };

            await _repository.SaveAuditAsync(audit);

            return Ok("Withdrawal successful");
        }
    }
}
