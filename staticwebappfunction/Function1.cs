using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace staticwebappfunction
{
    public static class Function1
    {
        private static Container _container;

        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            await SetUpDatabaseConnection();

            return new OkObjectResult(JsonConvert.SerializeObject(await DoCosmostStuff()));
        }

        private static async Task SetUpDatabaseConnection()
        {
            var cosmosClient = new CosmosClient("EnterYourURIHere", 
                "EnterYourPrimaryKeyHere", new CosmosClientOptions());
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync("StaticWebAppDatabase");
            _container = await database.CreateContainerIfNotExistsAsync("Products", "/Name", 400);
        }

        private static async Task<List<Product>> DoCosmostStuff()
        {
            var feedIterator = _container.GetItemQueryIterator<Product>();
            var products = new List<Product>();

            while (feedIterator.HasMoreResults)
            {
                foreach (var item in await feedIterator.ReadNextAsync())
                {
                    {
                        products.Add(item);
                    }
                }
            }

            return products;
        }
    }
}