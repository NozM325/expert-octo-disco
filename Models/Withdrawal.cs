using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace bankassessment.Models
{
    public class Withdrawal
    {
    public decimal Amount { get; set; }
    public long AccountId { get; set; }
    public string Status { get; set; }

    public Withdrawal(decimal amount, long accountId, string status)
    {
        Amount = amount;
        AccountId = accountId;
        Status = status;
    }

    public string ToJson() => JsonSerializer.Serialize(this);
    }
}