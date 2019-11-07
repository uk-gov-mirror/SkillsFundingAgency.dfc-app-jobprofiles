using DFC.App.JobProfile.Data.Models;
using DFC.App.JobProfile.MessageFunctionApp.HttpClientPolicies;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DFC.App.JobProfile.MessageFunctionApp.Services
{
    public class RefreshHttpClientService : IRefreshHttpClientService
    {
        private readonly HttpClient httpClient;
        private readonly JobProfileClientOptions jobProfileClientOptions;
        private readonly ILogger logger;

        public RefreshHttpClientService(JobProfileClientOptions jobProfileClientOptions, HttpClient httpClient, ILogger logger)
        {
            this.jobProfileClientOptions = jobProfileClientOptions;
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<HttpStatusCode> PostRefreshAsync(RefreshJobProfileSegment postModel)
        {
            if (postModel is null)
            {
                throw new ArgumentNullException(nameof(postModel));
            }

            var url = new Uri($"{jobProfileClientOptions.BaseAddress}refresh");
            using (var content = new ObjectContent<RefreshJobProfileSegment>(postModel, new JsonMediaTypeFormatter(), MediaTypeNames.Application.Json))
            {
                var response = await httpClient.PostAsync(url, content).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    logger.LogError($"Failure status code '{response.StatusCode}' received with content '{responseContent}', for POST type {typeof(RefreshJobProfileSegment)}, Id: {postModel.JobProfileId}.");

                    response.EnsureSuccessStatusCode();
                }

                return response.StatusCode;
            }
        }
    }
}