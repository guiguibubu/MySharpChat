using System.Net.Http;
using System.Threading.Tasks;
using MySharpChat.Core.Constantes;
using MySharpChat.Core.Model;

namespace MySharpChat.Core.API
{
    [RestEase.BasePath(ApiConstantes.API_PREFIX + "/" + ApiConstantes.API_MESSAGE_PREFIX)]
    [RestEase.AllowAnyStatusCode]
    public interface IMessagesApi
    {
        [RestEase.Post()]
        public Task<HttpResponseMessage> PostMessageAsync([RestEase.Query] string? userId, [RestEase.Body] ChatMessage chatMessage);
    }
}
