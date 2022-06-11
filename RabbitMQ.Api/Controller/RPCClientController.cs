using Microsoft.AspNetCore.Mvc;
using RabbitMQ.EventBus.Producer;

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
