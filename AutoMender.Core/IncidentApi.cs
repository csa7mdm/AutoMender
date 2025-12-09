using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AutoMender.Core
{
    public class IncidentApi
    {
        private readonly IncidentStore _store;

        public IncidentApi(IncidentStore store)
        {
            _store = store;
        }

        [Function("GetIncidents")]
        public async Task<HttpResponseData> GetIncidents([HttpTrigger(AuthorizationLevel.Function, "get", Route = "incidents")] HttpRequestData req)
        {
            var incidents = _store.GetIncidents();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(incidents);
            return response;
        }
    }
}
