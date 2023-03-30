using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IM_DataAccess.Models
{
    public class IncidentsWithPage
    {
        public int Total_Incidents { get; set; }
        public List<Incident> Incidents { get; set; }
    }
}
