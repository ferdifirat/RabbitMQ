using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.EventBus.Core;
using RabbitMQ.EventBus.Producer;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RabbitMQ.Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProducerController : ControllerBase
    {
        private readonly EventBusRabbitMQProducer _eventBusRabbitMQProducer;

        public ProducerController(EventBusRabbitMQProducer eventBusRabbitMQProducer)
        {
            _eventBusRabbitMQProducer = eventBusRabbitMQProducer;
        }

        [HttpPost("Publish")]
        public async Task<ActionResult> Publish([FromBody] string message)
        {
            _eventBusRabbitMQProducer.Publish($"{EventBusConstants.DirectQueue}", JsonConvert.SerializeObject(message));
            return Ok();
        }
    }
}

