using System.Net.Http;
using System.Threading.Tasks;
using MySharpChat.Core.Constantes;

namespace MySharpChat.Core.API
{
    [RestEase.BasePath(ApiConstantes.API_PREFIX + "/" + ApiConstantes.API_EVENT_PREFIX)]
    [RestEase.AllowAnyStatusCode]
    public interface IEventsApi
    {
        [RestEase.Get()]
        public Task<HttpResponseMessage> GetEventsAsync([RestEase.Query] string? userId, [RestEase.Query] string? lastId);
    }
}
