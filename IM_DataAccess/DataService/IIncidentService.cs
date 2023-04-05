using IM_DataAccess.Models;

namespace IM_DataAccess.DataService
{
    public interface IIncidentService
    {
        Task<Comment> AddCommentAsync(Comment comment);
        Task<CommentAttachments> AddCommentAttachmentsAsync(CommentAttachments commentAttachments);
        Task<Incident> AddIncident(Incident incident);
        Task<IncidentAttachments> AddIncidentAttachmentsAsync(IncidentAttachments incidentAttachments);
        Task<bool> DeleteCommentAsync(string commentId, string userId);
        Task<Comment> GetCommentByIdAsync(string commentId);
        Task<string> DeleteFileAsync(string type, string filetId, string userId);
        Task<List<IncidentAttachments>> GetIncidentAttachmentAsync(string incidentId);
        Task<Incident> GetIncidentrByIdAsync(string incidentId);
        Task<IncidentsWithPage> GetIncidentsPageAsync(int pageSize, int pageNumber, string? sortBy, string? sortDirection, string? serach);
        Task<bool> UpdateCommentAsync(string commentId, string commentText, string userId);
        Task<bool> UpdateIncidentAsync(string incidentId, string parameter, string value, string userId);
        void DeleteDirectory(string path);

        Task<object> KPIAsync(string userId);
        Task<object> OverallWidgetAsync();
        Task<List<Incident>> Last5IncidentsAsync();
        Task<List<Incident>> Oldest5UnresolvedIncidentsAsync();
        Task<object> MostAssignedToUsersIncidentsAsync();
    }
}