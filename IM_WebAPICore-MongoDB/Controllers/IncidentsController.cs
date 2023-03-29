using IM_DataAccess.DataService;
using IM_DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IM_WebAPICore_MongoDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentsController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IIncidentService _incidentService;
        public IncidentsController(IWebHostEnvironment webHostEnvironment, IIncidentService incidentService)
        {
            _webHostEnvironment = webHostEnvironment;
            _incidentService = incidentService;
        }

        [HttpPost("AddIncident")]
       // [Authorize]
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


    }
}
