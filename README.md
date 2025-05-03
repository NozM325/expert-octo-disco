# 💸 expert-octo-disco

`expert-octo-disco` is a local c#-based banking simulation project that integrates with AWS services like **DynamoDB** and **SNS**. It allows users to perform bank account withdrawals and then publishes the transaction result to an SNS topic.
---

## 🚀 API Endpoints

### `POST /bank/withdraw`

Withdraws a specified amount from an account if sufficient funds exist.

**Request Parameters:**
- `accountId`: ID of the account
- `amount`: Amount to withdraw

**Responses:**
- `200 OK`: Withdrawal successful
- `400 Bad Request`: Insufficient funds
- `500 Internal Server Error`: Withdrawal failed

---

## 🧩 Java Code

```java
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.*;
import org.springframework.jdbc.core.JdbcTemplate;
import software.amazon.awssdk.regions.Region;
import software.amazon.awssdk.services.sns.SnsClient;
import software.amazon.awssdk.services.sns.model.PublishRequest;
import software.amazon.awssdk.services.sns.model.PublishResponse;
import java.math.BigDecimal;

@RestController
@RequestMapping("/bank")
public class BankAccountController {

    @Autowired
    private JdbcTemplate jdbcTemplate;

    private final SnsClient snsClient;

    public BankAccountController() {
        this.snsClient = SnsClient.builder()
            .region(Region.YOUR_REGION) // Replace with your actual AWS region
            .build();
    }

    @PostMapping("/withdraw")
    public String withdraw(@RequestParam("accountId") Long accountId,
                           @RequestParam("amount") BigDecimal amount) {
        // Check current balance
        String sql = "SELECT balance FROM accounts WHERE id = ?";
        BigDecimal currentBalance = jdbcTemplate.queryForObject(sql, new Object[]{accountId}, BigDecimal.class);

        if (currentBalance != null && currentBalance.compareTo(amount) >= 0) {
            // Update balance
            sql = "UPDATE accounts SET balance = balance - ? WHERE id = ?";
            int rowsAffected = jdbcTemplate.update(sql, amount, accountId);

            if (rowsAffected > 0) {
                // Create and publish event
                WithdrawalEvent event = new WithdrawalEvent(amount, accountId, "SUCCESSFUL");
                String eventJson = event.toJson();

                String snsTopicArn = "arn:aws:sns:YOUR_REGION:YOUR_ACCOUNT_ID:YOUR_TOPIC_NAME"; // Replace with actual ARN

                PublishRequest publishRequest = PublishRequest.builder()
                    .message(eventJson)
                    .topicArn(snsTopicArn)
                    .build();

                snsClient.publish(publishRequest);

                return "Withdrawal successful";
            } else {
                return "Withdrawal failed";
            }
        } else {
            return "Insufficient funds for withdrawal";
        }
    }
}

class WithdrawalEvent {
    private BigDecimal amount;
    private Long accountId;
    private String status;

    public WithdrawalEvent(BigDecimal amount, Long accountId, String status) {
        this.amount = amount;
        this.accountId = accountId;
        this.status = status;
    }

    public BigDecimal getAmount() {
        return amount;
    }

    public Long getAccountId() {
        return accountId;
    }

    public String getStatus() {
        return status;
    }

    public String toJson() {
        return String.format("{\"amount\":\"%s\",\"accountId\":%d,\"status\":\"%s\"}",
                amount, accountId, status);
    }
}
