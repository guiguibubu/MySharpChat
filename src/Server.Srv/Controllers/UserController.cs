using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MySharpChat.Core.Constantes;
using MySharpChat.Core.Model;
using MySharpChat.Core.Packet;

namespace MySharpChat.Server.Srv.Controllers
{
    [ApiController]
    [Route(ApiConstantes.API_PREFIX + "/" + ApiConstantes.API_USER_PREFIX)]
    [ApiVersion("1.0")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IServerImpl _server;

        public UserController(ILogger<UserController> logger, IServerImpl server)
        {
            _logger = logger;
            _server = server;
        }

        [HttpGet("", Name = "GetUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get([FromQuery] string? userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                string errorMessage = "Request must have a \"userId\" param";
                _logger.LogError(errorMessage);

                return BadRequest(errorMessage);
            }
            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                string errorMessage = "Request parameter \"userId\" must respect GUID format";
                _logger.LogError(errorMessage);

                return BadRequest(errorMessage);
            }

            if (!_server.ChatRoom.IsUserConnected(userIdGuid))
            {
                string errorMessage = $"User with userId \"{userId}\" must be connected before reading any info";
                _logger.LogError(errorMessage);

                return NotFound(errorMessage);
            }

            IEnumerable<PacketWrapper<User>> packets = _server.ChatRoom.GetUserPackets();

            string responseContent = PacketSerializer.Serialize(packets);
            return Ok(responseContent);
        }

        [HttpPut("{userId?}", Name = "PutUser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Put([FromRoute] string? userId, [FromBody] User? userInfo)
        {
            if (string.IsNullOrEmpty(userId))
            {
                string errorMessage = "Request must have a \"userId\" param";
                _logger.LogError(errorMessage);

                return BadRequest(errorMessage);
            }
            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                string errorMessage = "Request parameter \"userId\" must respect GUID format";
                _logger.LogError(errorMessage);

                return BadRequest(errorMessage);
            }

            if (!_server.ChatRoom.IsUserConnected(userIdGuid))
            {
                string errorMessage = $"User with userId \"{userId}\" must be connected to be modified";
                _logger.LogError(errorMessage);

                return NotFound(errorMessage);
            }

            if (userInfo is null)
            {
                string errorMessage = $"Message request body must not be empty";
                _logger.LogError(errorMessage);

                return BadRequest(errorMessage);
            }

            string newUsername = userInfo.Username;
            if (_server.ChatRoom.ModifyUser(userIdGuid, newUsername))
            {
                return NoContent();
            }
            else
            {
                string errorMessage = $"This username is already used : \"{newUsername}\"";

                return Conflict(errorMessage);
            }
        }
    }
}