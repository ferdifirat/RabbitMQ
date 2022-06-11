using Microsoft.AspNetCore.Mvc;
using RabbitMQ.EventBus.Producer;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RabbitMQ.Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class RPCClientController : ControllerBase
    {
        private readonly RpcClient _rpcClient;
        public RPCClientController(RpcClient rpcClient)
        {
            _rpcClient = rpcClient;
        }

        [HttpPost("GetMessageFromClient")]
        public async Task<ActionResult> GetMessageFromClient([FromBody] string message)
        {
            var response = _rpcClient.Call(message);
            return Ok(response);
        }
    }
}
