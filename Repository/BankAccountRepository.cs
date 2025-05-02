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
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly IAmazonDynamoDB _dynamoDb;

        public BankAccountRepository(IAmazonDynamoDB dynamoDb)
        {
            _dynamoDb = dynamoDb;
        }

        public async Task<Document> GetAccountAsync(string accountId)
        {
            var table = Table.LoadTable(_dynamoDb, "tblBank");
            return await table.GetItemAsync(accountId);
        }

        public async Task UpdateAccountAsync(Document document)
        {
            var table = Table.LoadTable(_dynamoDb, "tblBank");
            await table.PutItemAsync(document);
        }

        public async Task SaveAuditAsync(Audit audit)
        {
            var auditTable = Table.LoadTable(_dynamoDb, "tblAudit");
            await auditTable.PutItemAsync(audit.ToDocument());
        }
    }

}