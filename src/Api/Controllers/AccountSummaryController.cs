using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Awesome_CMDB_DataAccess_Models;
using Meilisearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly ILogger _logger;

        public AccountsController(ILogger<AccountsController> logger)
        {
            _logger = logger;
        }


        /// <summary>
        /// Get the account summary listing accounts and username etc.
        /// </summary>
        /// <returns>AwesomeAccountSummary</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Account>), 200)]
        public IActionResult Get()
        {
            var accountId = User.GetAccountId();
            var accountList = new List<Account>
            {
                new Account
                {
                    Id = "AWS-exampleAccountNumber-00001",
                    AccountName = "My test AWS account",
                    DatacenterType = DatacenterType.AWS
                },
                new Account
                {
                    Id = "AWS-exampleAccountNumber-00002",
                    AccountName = "My other test AWS account",
                    DatacenterType = DatacenterType.AWS
                }
            };

            return new JsonResult(accountList);

        }


        /// <summary>
        /// Sets the account summary listing accounts and username etc.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Account awesomeAccount)
        {
            awesomeAccount.UserAccountId = User.GetAccountId();
            _logger.LogInformation("Storing an account");
            _logger.LogInformation($"{awesomeAccount.Id} - {awesomeAccount.AccountName}");
            _logger.LogInformation($"Contains {awesomeAccount.ServerGroups.Count} Server groups, with a total of {awesomeAccount.ServerGroups.SelectMany(a=> a.Servers).Count()} servers");


            var client = new MeilisearchClient("http://10.0.50.74:7700", "masterKey");

            var index = client.Index("Accounts");
            var documents = new List<Account> { awesomeAccount };
            var task = await index.AddDocumentsAsync(documents);

            var servers = awesomeAccount.ServerGroups.SelectMany(s => s.Servers);
            await client.Index("Servers").AddDocumentsAsync(servers);
            _logger.LogDebug(task.Status);
            return Ok();
        }
    }




}
