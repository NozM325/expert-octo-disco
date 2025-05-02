using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using bankassessment.Models;

namespace bankassessment.Repository
{
    //Implementing classes handle communication with DynamoDB
    public interface IBankAccountRepository
    {
        // Retrieves a bank account document by account ID from the 'tblBank' table
        Task<Document> GetAccountAsync(string accountId);

        // Updates or saves a bank account document in the 'tblBank' table.
        Task UpdateAccountAsync(Document document);

        // Saves an audit record to the 'tblAudit' table.
        Task SaveAuditAsync(Audit audit);
    }

}