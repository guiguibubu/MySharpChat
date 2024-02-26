using System.Net.Http;
using System.Threading.Tasks;
using MySharpChat.Core.Constantes;

namespace MySharpChat.Core.API
{
    [RestEase.BasePath(ApiConstantes.API_PREFIX + "/" + ApiConstantes.API_CONNEXION_PREFIX)]
    [RestEase.AllowAnyStatusCode]
    public interface IConnexionsApi
    {
        [RestEase.Get()]
        public Task<HttpResponseMessage> GetConnexionAsync([RestEase.Query] string? userId);

        [RestEase.Post()]
        public Task<HttpResponseMessage> PostConnexionAsync([RestEase.Query] string? userId, [RestEase.Query] string? username);

        [RestEase.Delete()]
        public Task<HttpResponseMessage> DeleteConnexionAsync([RestEase.Query] string? userId);
    }
}
