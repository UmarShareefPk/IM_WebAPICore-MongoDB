using IM_DataAccess.Models;

namespace IM_DataAccess.DataService
{
    public interface IIncidentService
    {
        Task<Incident> AddIncident(Incident incident);
        Task<IncidentAttachments> AddIncidentAttachmentsAsync(IncidentAttachments incidentAttachments);
    }
}