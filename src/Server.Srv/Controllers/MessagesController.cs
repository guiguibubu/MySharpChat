using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MySharpChat.Core.Constantes;
using MySharpChat.Core.Model;
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
        public IActionResult Post([FromQuery] string? userId, [FromBody] ChatMessage chatMessage)
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
            if (chatMessage is null)
            {
                string errorMessage = $"Message request body must not be empty";
                _logger.LogError(errorMessage);

                return BadRequest(errorMessage);
            }

            _server.ChatRoom.AddMessage(userIdGuid, chatMessage);
            return Created($"{Request.Scheme}://{Request.Host}/{ApiConstantes.API_PREFIX}/{ApiConstantes.API_MESSAGE_PREFIX}/{chatMessage.Id}", null);
        }

        [HttpGet("{uid}", Name = "GetMessage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get([FromRoute] string? uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                string errorMessage = $"Message request must have a \"{nameof(uid)}\" param";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }
            if (!Guid.TryParse(uid, out Guid messageIdGuid))
            {
                string errorMessage = $"Message request parameter \"{nameof(uid)}\" must respect GUID format";
                _logger.LogError(errorMessage);
                return BadRequest(errorMessage);
            }
            ChatMessage? chatMessage = _server.ChatRoom.GetMessage(messageIdGuid);
            if(chatMessage is null)
            {
                string errorMessage = $"Message {uid} not found";
                _logger.LogError(errorMessage);

                return NotFound(errorMessage);
            }

            return Ok(chatMessage);
        }
    }
}