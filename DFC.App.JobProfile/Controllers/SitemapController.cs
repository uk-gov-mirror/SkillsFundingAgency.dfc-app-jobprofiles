﻿using DFC.App.JobProfile.Data.Providers;
using DFC.App.JobProfile.Extensions;
using DFC.App.JobProfile.Models;
using DFC.App.Services.Common.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DFC.App.JobProfile.Controllers
{
    public class SitemapController : Controller
    {
        private readonly ILogger<SitemapController> logService;
        private readonly IProvideJobProfiles jobProfileService;

        public SitemapController(ILogger<SitemapController> logService, IProvideJobProfiles jobProfileService)
        {
            this.logService = logService;
            this.jobProfileService = jobProfileService;
        }

        [HttpGet]
        public async Task<ContentResult> Sitemap()
        {
            try
            {
                logService.LogInformation($"{Utils.LoggerMethodNamePrefix()} Generating Sitemap");

                var sitemapUrlPrefix = $"{Request.GetBaseAddress()}{ProfileController.ProfilePathRoot}";
                var sitemap = new Sitemap();

                var jobProfileModels = await jobProfileService.GetAllItems();

                if (jobProfileModels != null)
                {
                    var jobProfileModelsList = jobProfileModels.ToList();

                    if (jobProfileModelsList.Any())
                    {
                        var sitemapJobProfileModels = jobProfileModelsList
                             .Where(w => w.IncludeInSitemap)
                             .OrderBy(o => o.CanonicalName);

                        foreach (var jobProfileModel in sitemapJobProfileModels)
                        {
                            sitemap.Add(new SitemapLocation
                            {
                                Url = $"{sitemapUrlPrefix}/{jobProfileModel.CanonicalName}",
                                Priority = 1,
                            });
                        }
                    }
                }

                // extract the sitemap
                var xmlString = sitemap.WriteSitemapToString();

                logService.LogInformation($"{Utils.LoggerMethodNamePrefix()} Generated Sitemap");

                return Content(xmlString, MediaTypeNames.Application.Xml);
            }
            catch (Exception ex)
            {
                logService.LogError($"{Utils.LoggerMethodNamePrefix()} {ex.Message}");
            }

            return null;
        }
    }
}