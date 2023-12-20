using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace MySharpChat.Server.Srv.Controllers
{
    [ApiController]
    [Route("api/messages")]
    [ApiVersion("1.0")]
    public class MessagesController : ControllerBase
    {
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(ILogger<MessagesController> logger)
        {
            _logger = logger;
        }

        [HttpPost(Name = "PostMessage")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Post()
        {
            return Created("toto", null);
        }
    }
}