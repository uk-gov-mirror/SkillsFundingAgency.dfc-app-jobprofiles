using AutoMapper;
using DFC.App.JobProfile.Data.Models;
using DFC.App.JobProfile.MessageFunctionApp.Extensions;
using DFC.App.JobProfile.MessageFunctionApp.HttpClientPolicies;
using DFC.App.JobProfile.MessageFunctionApp.Services;
using DFC.Functions.DI.Standard;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

[assembly: WebJobsStartup(typeof(DFC.App.JobProfile.MessageFunctionApp.Startup.WebJobsExtensionStartup), "Web Jobs Extension Startup")]

namespace DFC.App.JobProfile.MessageFunctionApp.Startup
{
    public class WebJobsExtensionStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables()
               .Build();

            builder?.AddDependencyInjection();
            builder?.Services.AddSingleton(configuration.GetSection("jobProfileClientOptions").Get<JobProfileClientOptions>());
            builder?.Services.AddSingleton(new HttpClient());
            builder?.Services.AddAutoMapper(typeof(WebJobsExtensionStartup).Assembly);
            builder?.Services.AddSingleton<IMessageProcessor, MessageProcessor>();
            builder?.Services.AddSingleton<ILogger, Logger<JobProfileModel>>();
            builder?.Services.AddSingleton<IHttpClientService<JobProfileModel>, HttpClientService<JobProfileModel>>();
            builder?.Services.AddSingleton<IRefreshHttpClientService, RefreshHttpClientService>();

            AddPollyConfiguration(builder.Services, configuration);
        }

        private void AddPollyConfiguration(IServiceCollection services, IConfigurationRoot configuration)
        {
            const string AppSettingsRefreshPolicies = "RefreshPolicies";
            const string AppSettingsPolicies = "Policies";
            var refreshPolicyOptions = configuration.GetSection(AppSettingsRefreshPolicies).Get<PolicyOptions>();
            //var policyOptions = configuration.GetSection(AppSettingsPolicies).Get<PolicyOptions>();
            var policyRegistry = services.AddPolicyRegistry();

            services
                .AddPolicies(policyRegistry, nameof(JobProfileClientOptions), refreshPolicyOptions)
                .AddHttpClient<IRefreshHttpClientService, RefreshHttpClientService, JobProfileClientOptions>(configuration, nameof(JobProfileClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            //services
            //    .AddPolicies(policyRegistry, nameof(JobProfileClientOptions), policyOptions)
            //    .AddHttpClient<IHttpClientService<JobProfileClientOptions>, HttpClientService<JobProfileClientOptions>, JobProfileClientOptions>(configuration, nameof(JobProfileClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));
        }
    }
}