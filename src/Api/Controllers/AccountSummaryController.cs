using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Awesome_CMDB_DataAccess_Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("accountSummary")]
    public class AccountSummaryController : ControllerBase
    {

        /// <summary>
        /// Get the account summary listing accounts and username etc.
        /// </summary>
        /// <returns>AwesomeAccountSummary</returns>
        [HttpGet]
        public IActionResult Get()
        {
            var accountId = User.GetAccountId();
            var accountList = new List<Account>
            {
                new Account
                {
                    AccountId = "AWS-exampleAccountNumber-00001",
                    AccountName = "My test AWS account",
                    DatacenterType = DatacenterType.AWS
                },
                new Account
                {
                    AccountId = "AWS-exampleAccountNumber-00002",
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
        public IActionResult Post(Account awesomeAccount)
        {
            awesomeAccount.AccountId = User.GetAccountId();
            //TODO save awesomeAccount somewhere


            return Ok();
        }
    }




}
