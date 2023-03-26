using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IM.Models
{
    public class IncidentLogs
    {
        public string Id { get; set; }
        public string IncidentId { get; set; }
        public string Parameter { get; set; }
        public string Value { get; set; }
        public string UserId { get; set; }
        public string OldValue { get; set; }
        public DateTime UpdateDate { get; set; }
     }
}