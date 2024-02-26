using System.Net.Http;
using System.Threading.Tasks;
using MySharpChat.Core.Constantes;
using MySharpChat.Core.Model;

namespace MySharpChat.Core.API
{
    [RestEase.BasePath(ApiConstantes.API_PREFIX + "/" + ApiConstantes.API_USER_PREFIX)]
    [RestEase.AllowAnyStatusCode]
    public interface IUsersApi
    {
        [RestEase.Get()]
        public Task<HttpResponseMessage> GetUsersAsync([RestEase.Query] string? userId);

        [RestEase.Put("{userId}")]
        public Task<HttpResponseMessage> PutUserAsync([RestEase.Path] string? userId, [RestEase.Body] User? userInfo);
    }
}
