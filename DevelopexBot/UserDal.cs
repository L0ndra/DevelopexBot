using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace DevelopexBot
{
    [Serializable]
    public class UserModel : TableEntity
    {
        public UserModel(string id)
        {
            this.PartitionKey = id;
            this.RowKey = id;
            Id = id;
        }

        public UserModel()
        {
            
        }
        public string Id { get; set; }
        public string Name { get; set; }

        public string Activity { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class UserDal
    {
        private static readonly Lazy<CloudTableClient> TableClient = new Lazy<CloudTableClient>(() =>
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
            return storageAccount.CreateCloudTableClient();
        }, LazyThreadSafetyMode.PublicationOnly);

        private const string TableName = "Users";

        public static async Task AddNewUserToTable(UserModel model)
        {
            // Retrieve a reference to the table.
            CloudTable table = TableClient.Value.GetTableReference(TableName);
            // Create the table if it doesn't exist.
            table.CreateIfNotExists();

            await table.ExecuteAsync(TableOperation.InsertOrReplace(model));
        }

        public static async Task<UserModel> GetUserAsync(string name)
        {
            CloudTable table = TableClient.Value.GetTableReference(TableName);

            TableResult res = await table.ExecuteAsync(TableOperation.Retrieve<UserModel>(name, name));

            if (res.Result == null)
            {
                return null;
            }
            return (UserModel) res.Result;
        }

        public static async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            CloudTable table = TableClient.Value.GetTableReference(TableName);

            TableContinuationToken token = new TableContinuationToken();
            var results = new List<UserModel>();
            while (token != null)
            {
                var res = await table.ExecuteQuerySegmentedAsync(new TableQuery<UserModel>(), token);
                token = res.ContinuationToken;
                results.AddRange(res.Results);
            }
            return results;
        }
    }
}