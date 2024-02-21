using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MySharpChat.Core.Constantes;
using MySharpChat.Core.Packet;

namespace MySharpChat.Server.Srv.Controllers
{
    [ApiController]
    [Route(ApiConstantes.API_PREFIX + "/" + ApiConstantes.API_CONNEXION_PREFIX)]
    [ApiVersion("1.0")]
    public class ConnexionController : ControllerBase
    {
        private readonly ILogger<ConnexionController> _logger;
        private readonly IServerImpl _server;

        public ConnexionController(ILogger<ConnexionController> logger, IServerImpl server)
        {
            _logger = logger;
            _server = server;
        }

        [HttpGet(Name = "GetConnexion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get([FromQuery] string? userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                string errorMessage = "Connection request must have a \"userId\" param";
                _logger.LogError(errorMessage);

                return BadRequest(errorMessage);
            }
            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                string errorMessage = "Connection request parameter \"userId\" must respect GUID format";
                _logger.LogError(errorMessage);

                return BadRequest(errorMessage);
            }

            if (_server.ChatRoom.IsUserConnected(userIdGuid))
            {
                return Ok($"{Request.Scheme}://{Request.Host}/api/user/{userIdGuid}");
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost(Name = "PostConnexion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Post([FromQuery] string? userId, [FromQuery] string? username)
        {
            if (string.IsNullOrEmpty(userId))
            {
                string errorMessage = "Connection request must have a \"userId\" param";
                _logger.LogError(errorMessage);

                return BadRequest(errorMessage);
            }
            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                string errorMessage = "Connection request parameter \"userId\" must respect GUID format";
                _logger.LogError(errorMessage);

                return BadRequest(errorMessage);
            }

            if (_server.ChatRoom.IsUserConnected(userIdGuid))
            {
                string errorMessage = $"User with userId \"{userId}\" already connected";
                _logger.LogError(errorMessage);

                return Conflict(errorMessage);
            }
            else
            {
                IEnumerable<PacketWrapper> responsePackets = _server.ChatRoom.ConnectUser(username, userIdGuid);
                string packetSerialized = PacketSerializer.Serialize(responsePackets);

                return Ok(packetSerialized);
            }
        }

        [HttpDelete(Name = "DeleteConnexion")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Delete([FromQuery] string? userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                string errorMessage = "Connection request must have a \"userId\" param";
                _logger.LogError(errorMessage);

                return BadRequest(errorMessage);
            }
            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                string errorMessage = "Connection request parameter \"userId\" must respect GUID format";
                _logger.LogError(errorMessage);

                return BadRequest(errorMessage);
            }

            if (_server.ChatRoom.IsUserConnected(userIdGuid))
            {
                _server.ChatRoom.DisconnectUser(userIdGuid);
            }
            return NoContent();
        }
    }
}