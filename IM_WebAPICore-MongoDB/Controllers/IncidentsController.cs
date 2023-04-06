using IM_DataAccess.DataService;
using IM_DataAccess.Models;
using IM_WebAPICore_MongoDB.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace IM_WebAPICore_MongoDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentsController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IIncidentService _incidentService;
        private readonly IMemoryCache _memoryCache;
        public IncidentsController(IWebHostEnvironment webHostEnvironment, IIncidentService incidentService, IMemoryCache memoryCache)
        {
            _webHostEnvironment = webHostEnvironment;
            _incidentService = incidentService;
            _memoryCache = memoryCache;
        }

        [HttpPost("AddIncident")]
        [Authorize]
        public async Task<IActionResult> AddIncident()
        {
            Incident incident = new Incident();
            incident.Title = HttpContext.Request.Form["Title"];
            incident.Description = HttpContext.Request.Form["Description"];
            incident.AdditionalData = HttpContext.Request.Form["AdditionalDeta"];
            incident.AssignedTo = HttpContext.Request.Form["AssignedTo"];
            incident.CreatedBy = HttpContext.Request.Form["CreatedBy"];
            incident.DueDate = DateTime.Parse(HttpContext.Request.Form["DueDate"]);
            incident.StartTime = DateTime.Parse(HttpContext.Request.Form["StartTime"]);
            incident.Status = HttpContext.Request.Form["Status"];

            // return null;

            DateTime dt = new DateTime();
            if (incident == null || string.IsNullOrWhiteSpace(incident.CreatedBy) || !DateTime.TryParse(incident.CreatedAT.ToString(), out dt)
                 || string.IsNullOrWhiteSpace(incident.AssignedTo) || string.IsNullOrWhiteSpace(incident.Title)
                 || string.IsNullOrWhiteSpace(incident.Description)
                 || !DateTime.TryParse(incident.StartTime.ToString(), out dt) || !DateTime.TryParse(incident.DueDate.ToString(), out dt)
                 || string.IsNullOrWhiteSpace(incident.Status)
                )
            {
                return BadRequest(new { message = "Please enter all required fields and make sure datetime fields are in correct format." });
            }

            string[] statusValidValues =
                                    {
                                            "N", // New 
                                            "O", // Open
                                            "I", // In Progress
                                            "C", // Closed
                                            "A"  // Approved
                                    };

            if (!statusValidValues.Contains(incident.Status.ToUpper()))
                return BadRequest(new { message = "Invalid Status Value. Valid values are N,O,I,C,A" });

            var dbResponse = await _incidentService.AddIncident(incident);

            //if (dbResponse.Error)
            //{
            //    if (dbResponse.ErrorMsg.Contains("FK_Incidents_CreatedBy"))
            //        return BadRequest(new { message = "This Creator Id does not exist in or system." });
            //    else if (dbResponse.ErrorMsg.Contains("FK_Incidents_AssignedTo"))
            //        return BadRequest(new { message = "This Assignee Id does not exist in or system." });
            //    else if (dbResponse.ErrorMsg.Contains("SqlDateTime overflow"))
            //        return BadRequest(new { message = "Incorrect Datetime value." });
            //    else
            //        return BadRequest(new { message = "Internal Error. " + dbResponse.ErrorMsg });
            //}

            string incident_Id = dbResponse.Id;

            if (HttpContext.Request.Form.Files.Count > 0)
            {
                foreach (var formFile in HttpContext.Request.Form.Files)
                {
                    string folder = _webHostEnvironment.ContentRootPath + "\\Attachments\\Incidents\\" + incident_Id;
                    Directory.CreateDirectory(folder);
                    if (formFile.Length > 0)
                    {
                        var attachment = new IncidentAttachments();
                        attachment.FileName = formFile.FileName;
                        attachment.ContentType = formFile.ContentType;
                        attachment.IncidentId = incident_Id;
                        string path = folder + "\\" + formFile.FileName;

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await formFile.CopyToAsync(stream);
                        }
                        await _incidentService.AddIncidentAttachmentsAsync(attachment);
                    }
                }

            }//end of if count > 0

            return Ok("New incident has been added.");
        }


        [HttpPost("AddComment")]
        [Authorize]
        public async Task<IActionResult> AddComment()
        {
            Comment comment = new Comment();
            comment.CommentText = HttpContext.Request.Form["CommentText"];
            comment.IncidentId = HttpContext.Request.Form["IncidentId"];
            comment.UserId = HttpContext.Request.Form["UserId"];

            DateTime dt = new DateTime();
            if (comment == null || string.IsNullOrWhiteSpace(comment.CommentText) || string.IsNullOrWhiteSpace(comment.IncidentId)
                )
            {
                return BadRequest(new { message = "Please enter all required fields." });
            }
            var commentAdded = await _incidentService.AddCommentAsync(comment);

            string comment_Id = commentAdded.Id;

            if (HttpContext.Request.Form.Files.Count > 0)
            {
                foreach (var formFile in HttpContext.Request.Form.Files)
                {
                    string folder = _webHostEnvironment.ContentRootPath + "\\Attachments\\Incidents\\" + comment.IncidentId + "\\Comments\\" + comment_Id;
                    Directory.CreateDirectory(folder);
                    if (formFile.Length > 0)
                    {
                        var attachment = new CommentAttachments();
                        attachment.FileName = formFile.FileName;
                        attachment.ContentType = formFile.ContentType;
                        attachment.CommentId = comment_Id;

                        string path = folder + "\\" + formFile.FileName;

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await formFile.CopyToAsync(stream);
                        }
                        await _incidentService.AddCommentAttachmentsAsync(attachment);
                    }
                }
            }//end of if count > 0

            var newComment = await _incidentService.GetCommentByIdAsync(comment_Id);
            return Ok(newComment);
        }


        [HttpGet("IncidentById")]
        [Authorize]
        public async Task<IActionResult> IncidentByIdAsync(string Id)
        {
            //Thread.Sleep(2000);
            return Ok(await _incidentService.GetIncidentrByIdAsync(Id));
        }

        [HttpGet("DownloadFile")]
        public ActionResult DownloadFile(string type, string? commentId, string incidentId, string filename, string contentType)
        {
            string ContentType = contentType;
            //Physical Path of Root Folder
            var rootPath = "";
            if (type.ToLower() == "comment")
            {
                rootPath = _webHostEnvironment.ContentRootPath + "\\Attachments\\Incidents\\" + incidentId + "\\Comments\\" + commentId;
            }
            else
            {
                rootPath = _webHostEnvironment.ContentRootPath + "\\Attachments\\Incidents\\" + incidentId;
            }
            var fileFullPath = Path.Combine(rootPath, filename);

            byte[] fileBytes = System.IO.File.ReadAllBytes(fileFullPath);
            return File(fileBytes, ContentType, filename);
        }

        [HttpGet("DeleteFile")]
        [Authorize]
        public async Task<string> DeleteFileAsync(string type, string commentId, string incidentId, string userId, string fileId, string filename, string contentType)
        {
            string ContentType = contentType;
            //Physical Path of Root Folder
            var rootPath = "";
            if (type.ToLower() == "comment")
            {
                await _incidentService.DeleteFileAsync("comment", fileId, userId);
                rootPath = _webHostEnvironment.ContentRootPath + "\\Attachments\\Incidents\\" + incidentId + "\\Comments\\" + commentId;
                // rootPath = System.Web.HttpContext.Current.Server.MapPath("~/Attachments/Incidents/" + incidentId + "/Comments/" + commentId);
            }
            else
            {
                await _incidentService.DeleteFileAsync("incident", fileId, userId);
                rootPath = _webHostEnvironment.ContentRootPath + "\\Attachments\\Incidents\\" + incidentId;
                //rootPath = System.Web.HttpContext.Current.Server.MapPath("~/Attachments/Incidents/" + incidentId);
            }

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            var fileFullPath = System.IO.Path.Combine(rootPath, filename);

            if (System.IO.File.Exists(@fileFullPath))
            {
                System.IO.File.Delete(@fileFullPath);
            }
            return "Fiile Delete";
        }


        [HttpGet("DeleteComment")]
        [Authorize]
        public async Task<string> DeleteCommentAsync(string commentId, string incidentId, string userId)
        {
            await _incidentService.DeleteCommentAsync(commentId, userId);
            string path = _webHostEnvironment.ContentRootPath + "\\Attachments\\Incidents\\" + incidentId + "\\Comments\\" + commentId;

            if (Directory.Exists(@path))
            {
                _incidentService.DeleteDirectory(@path);
            }
            return "Comment Delete";
        }


        [HttpPost("UpdateIncident")]
        [Authorize]
        public async Task UpdateIncidentAsync([FromBody] IncidentUpdate IU) //IU = IncidentUpdate, 
        {
            await _incidentService.UpdateIncidentAsync(IU.IncidentId, IU.Parameter, IU.Value, IU.UserId);
        }

        [HttpPost("UpdateComment")]
        [Authorize]
        public async Task UpdateCommentAsync([FromBody] Comment C)
        {
            await _incidentService.UpdateCommentAsync(C.Id, C.CommentText, C.UserId);
        }

        [Authorize]
        [HttpGet("GetIncidentsWithPage")]
        public async Task<object> GetIncidentsWithPageAsync(int PageSize = 5, int PageNumber = 1, string SortBy = "a", string SortDirection = "a", string? Search = "")
        {
            //Thread.Sleep(000);
            var response = await _incidentService.GetIncidentsPageAsync(PageSize, PageNumber, SortBy, SortDirection, Search);
            return response;
        }

        [Authorize]
        [HttpGet("KPI")]
        public async Task<object> GetKPIAsync(string UserId)
        {
            return await _incidentService.KPIAsync(UserId);
        }

        [Authorize]
        [HttpGet("OverallWidget")]
        public async Task<object> GetOverallWidgetAsync(string? UserId)
        {
            return await _incidentService.OverallWidgetAsync();
        }

        [Authorize]
        [HttpGet("Last5Incidents")]
        public async Task<object> GetLast5IncidentsAsync(string? UserId)
        {
            return await _incidentService.Last5IncidentsAsync();
        }

        [Authorize]
        [HttpGet("Oldest5UnresolvedIncidents")]
        public async Task<object> GetOldest5UnresolvedIncidentsAsync(string? UserId)
        {
            return await _incidentService.Oldest5UnresolvedIncidentsAsync();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("MostAssignedToUsersIncidents")]
        public async Task<object> GetMostAssignedToUsersIncidentsAsync(string? UserId)
        {
            return await _incidentService.MostAssignedToUsersIncidentsAsync();
        }
    }//end of class
}
