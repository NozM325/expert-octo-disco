using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using bankassessment;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using bankassessment.Models;

namespace bankassessment.Repository
{
    // It provides data access methods for working with the DynamoDB tables
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly IAmazonDynamoDB _dynamoDb;


        // Constructor receives an instance of IAmazonDynamoDB
        public BankAccountRepository(IAmazonDynamoDB dynamoDb)
        {
            _dynamoDb = dynamoDb;
        }

        public async Task<Document> GetAccountAsync(string accountId)
        {
            // Retrieves an account document from the 'tblBank' table based on the accountId
            var table = Table.LoadTable(_dynamoDb, "tblBank");
            return await table.GetItemAsync(accountId);
        }

        public async Task UpdateAccountAsync(Document document)
        {
            // Updates an existing account document in the 'tblBank' table
            var table = Table.LoadTable(_dynamoDb, "tblBank");
            await table.PutItemAsync(document);
        }

        public async Task SaveAuditAsync(Audit audit)
        {
            // Saves an audit record to the 'tblAudit' table
            // The audit is converted to a DynamoDB document using the ToDocument() method in the Audit mode
            var auditTable = Table.LoadTable(_dynamoDb, "tblAudit");
            await auditTable.PutItemAsync(audit.ToDocument());
        }
    }

}