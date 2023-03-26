using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IM.Models
{
    public class IncidentAttachments
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public DateTime DateAdded { get; set; }
        public string IncidentId { get; set; }
    }
}