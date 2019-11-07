using DFC.App.JobProfile.Data.Models;
using System.Net;
using System.Threading.Tasks;

namespace DFC.App.JobProfile.MessageFunctionApp.Services
{
    public interface IRefreshHttpClientService
    {
        Task<HttpStatusCode> PostRefreshAsync(RefreshJobProfileSegment postModel);
    }
}