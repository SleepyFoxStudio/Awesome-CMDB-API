using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("accountSummary")]
    public class AccountSummaryController : ControllerBase
    {
        private readonly IDocumentStore _documentStore;

        public AccountSummaryController(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        /// <summary>
        /// Get the account summary listing accounts and username etc.
        /// </summary>
        /// <returns>AwesomeAccountSummary</returns>
        [HttpGet]
        public IActionResult Get()
        {

            using (var session = _documentStore.OpenSession())
            {
                return new JsonResult(session
                    .Query<AwesomeAccount>()
                    .Where(x => x.AccountId == User.GetAccountId()));
            }
        }


        /// <summary>
        /// Sets the account summary listing accounts and username etc.
        /// </summary>
        [HttpPost]
        public IActionResult Post(AwesomeAccount awesomeAccount)
        {
            awesomeAccount.AccountId = User.GetAccountId();
            using (var session = _documentStore.LightweightSession())
            {
                session.Store(awesomeAccount);
                session.SaveChanges();
            }

            return Ok();
        }
    }


    public class AwesomeAccountSummary
    {
        public List<AwesomeAccount> Accounts { get; set; } = new List<AwesomeAccount>();
        public string User { get; set; }
    }

    public class AwesomeAccount
    {
        private string _accountId;
        public string Name { get; set; }
        public string Id { get; set; }
        public string DataCentreType { get; set; }


        public string AccountId { get; set; }

     
    }

}
