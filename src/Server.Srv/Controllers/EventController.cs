using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MySharpChat.Core.Constantes;
using MySharpChat.Core.Packet;

namespace MySharpChat.Server.Srv.Controllers
{
    [ApiController]
    [Route(ApiConstantes.API_PREFIX + "/" + ApiConstantes.API_EVENT_PREFIX)]
    [ApiVersion("1.0")]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly IServerImpl _server;

        public EventController(ILogger<EventController> logger, IServerImpl server)
        {
            _logger = logger;
            _server = server;
        }

        [HttpGet(Name = "GetEvents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Get([FromQuery] string? userId, [FromQuery] string? lastId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                string errorMessage = "Message request must have a \"userId\" param";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }
            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                string errorMessage = "Message request parameter \"userId\" must respect GUID format";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }

            if (!_server.ChatRoom.IsUserConnected(userIdGuid))
            {
                string errorMessage = $"User with userId \"{userId}\" must be connected before reading any event";
                _logger.LogError(errorMessage);

                return BadRequest(errorMessage);
            }

            IEnumerable<PacketWrapper> packets = _server.ChatRoom.GetChatEvents(lastId);
            string responseContent = PacketSerializer.Serialize(packets);
            return Ok(responseContent);
        }
    }
}