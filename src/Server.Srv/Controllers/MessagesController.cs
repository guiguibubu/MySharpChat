using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MySharpChat.Core.Constantes;
using MySharpChat.Core.Packet;

namespace MySharpChat.Server.Srv.Controllers
{
    [ApiController]
    [Route(ApiConstantes.API_PREFIX + "/" + ApiConstantes.API_MESSAGE_PREFIX)]
    [ApiVersion("1.0")]
    public class MessagesController : ControllerBase
    {
        private readonly ILogger<MessagesController> _logger;
        private readonly IServerImpl _server;

        public MessagesController(ILogger<MessagesController> logger, IServerImpl server)
        {
            _logger = logger;
            _server = server;
        }

        [HttpPost(Name = "PostMessage")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Post([FromQuery] string? userId, [FromBody] ChatMessagePacket chatPacket)
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
                string errorMessage = $"User with userId \"{userId}\" must be connected before sending any message";
                _logger.LogError(errorMessage);

                return BadRequest(errorMessage);
            }
            if (chatPacket == null)
            {
                string errorMessage = $"Message request body must not be empty";
                _logger.LogError(errorMessage);

                return BadRequest(errorMessage);
            }

            _server.ChatRoom.AddMessage(userIdGuid, chatPacket.ChatMessage);
            return Created($"{Request.Scheme}://{Request.Host}/api/messages/{chatPacket.ChatMessage.Id}", null);
        }
    }
}