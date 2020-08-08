using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("accountSummary")]
    public class AccountSummaryController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var dummyAccountSummary = new AwesomeAccountSummary {User = "Mr Joe Smith"};
            dummyAccountSummary.Accounts.Add(new AwesomeAccount
            {
                Name = "Test account 1",
                Id = "i-0000000001",
                DataCentreType = "AWS"
            });
            dummyAccountSummary.Accounts.Add(new AwesomeAccount
            {
                Name = "Test account 2",
                Id = "i-0000000002",
                DataCentreType = "AWS"
            });
            dummyAccountSummary.Accounts.Add(new AwesomeAccount
            {
                Name = "Test account 3",
                Id = "i-0000000003",
                DataCentreType = "VMWare"
            });
            return new JsonResult(dummyAccountSummary);
        }
    }


    public class AwesomeAccountSummary
    {
        public List<AwesomeAccount> Accounts { get; set; } = new List<AwesomeAccount>();
        public string User { get; set; }
    }

    public class AwesomeAccount
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string DataCentreType { get; set; }
    }

}
