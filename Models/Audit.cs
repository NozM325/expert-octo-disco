using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;

namespace bankassessment.Models
{
    public class Audit // Transaction tracker
    {
        public string TransactionId { get; set; } = Guid.NewGuid().ToString();
        public long AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public Document ToDocument() => new Document
        {
            ["TransactionId"] = TransactionId,
            ["AccountId"] = AccountId,
            ["Amount"] = Amount,
            ["Status"] = Status,
            ["Timestamp"] = Timestamp.ToString("o")
        };
    }
}