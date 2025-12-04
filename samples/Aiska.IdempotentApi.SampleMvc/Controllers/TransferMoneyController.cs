using Aiska.IdempotentApi.Attributes;
using Aiska.IdempotentApi.Filters;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Aiska.IdempotentApi.SampleMvc.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Idempotent]
    public class TransferMoneyController : ControllerBase
    {
        private static readonly string[] SampeData =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        [HttpGet(Name = "GetTramsferRecord")]
        public IEnumerable<DataTransfer> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new DataTransfer
            {
                AquirerName = SampeData[Random.Shared.Next(SampeData.Length)],
                IssuerName = SampeData[Random.Shared.Next(SampeData.Length)],
                Money = 10000000,
                Timestamp = DateTime.Now
            })
            .ToArray();
        }

        [HttpPost(Name = "TransferMoney")]
        public async Task<IActionResult> Post(
            [IdempotentExclude("Timestamp")] 
            DataTransfer data)
        {
            await Task.Delay(10 * 1000);
            return Created();
        }
    }
}
