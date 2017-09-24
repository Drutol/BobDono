using System.Net.Http;

namespace BobDono.Interfaces
{
    public interface IHttpClientProvider
    {
        HttpClient HttpClient { get; }
    }
}
