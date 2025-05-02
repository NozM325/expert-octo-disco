using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using bankassessment.Models;

namespace bankassessment.Repository
{
    public interface IBankAccountRepository
    {
        Task<Document> GetAccountAsync(string accountId);
        Task UpdateAccountAsync(Document document);
        Task SaveAuditAsync(Audit audit);
    }

}