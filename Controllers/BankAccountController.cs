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
            var document = await _repository.GetAccountAsync(accountId.ToString());

            if (amount <= 0)
            {
                return BadRequest("Amount must be greater than 0");
            }

            _logger.LogInformation("Withdrawing {Amount} from account {AccountId}", amount, accountId);

            if (document == null || !document.Contains("Balance"))
                return BadRequest("Account not found");

            var currentBalance = document["Balance"].AsDecimal();

            if (currentBalance < amount)
                return BadRequest("Insufficient funds");

            // Update balance
            document["Balance"] = currentBalance - amount;
            await _repository.UpdateAccountAsync(document);

            // Publish to SNS
            var withdrawal = new Withdrawal(amount, accountId, "SUCCESSFUL");
            var publishRequest = new PublishRequest
            {
                Message = withdrawal.ToJson(),
                TopicArn = _snsTopicArn,
                MessageGroupId = accountId.ToString(),
                MessageDeduplicationId = Guid.NewGuid().ToString()
            };

            try
            {
                await _snsClient.PublishAsync(publishRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SNS Publish failed");
                return StatusCode(500, "Internal server error");
            }

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
