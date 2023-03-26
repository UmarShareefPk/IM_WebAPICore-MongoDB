using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Models
{
    public class IncidentNotification
    {
        public string Id { get; set; }
        public string IncidentId { get; set; }
        public string SourceUserId { get; set; }
        public bool IsRead { get; set; }
        public DateTime ReadDate { get; set; }
        public DateTime CreateDate { get; set; }
        public string UserId { get; set; }
        public string NotifyAbout { get; set; }


    }
}
