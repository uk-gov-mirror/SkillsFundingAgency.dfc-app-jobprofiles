﻿using DFC.App.Services.Common.Registration;
using DFC.Content.Pkg.Netcore.Data.Models.ClientOptions;

namespace DFC.App.JobProfile.ContentAPI.Configuration
{
    internal sealed class ContentApiConfiguration :
        ClientOptionsModel,
        IContentApiConfiguration,
        IRequireConfigurationRegistration
    {
        public string SummaryEndpoint { get; set; } = "/page";

        public string StaticContentEndpoint { get; set; } = "/sharedcontent/";

        public string[] PageStaticContentIDs { get; set; }

        public string[] SupportedRelationships { get; set; }
    }
}
