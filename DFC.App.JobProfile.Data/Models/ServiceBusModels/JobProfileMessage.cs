using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DFC.App.JobProfile.Data.Models.ServiceBusModels
{
    [ExcludeFromCodeCoverage]
    public class JobProfileMessage
    {
        [Required]
        public Guid JobProfileId { get; set; }

        [Required]
        public string CanonicalName { get; set; }

        public IList<string> AlternativeNames { get; set; }

        [Required]
        public string SocLevelTwo { get; set; }

        [Required]
        public DateTime? LastModified { get; set; }

        public string Title { get; set; }

        public bool IncludeInSitemap { get; set; }

        public string Overview { get; set; }
    }
}
