
using Amazon.Runtime.Internal.Util;
using BogusData.Data;
using IM.Models;
using IM_DataAccess.DataService;
using IM_DataAccess.Models;
using IM_WebAPICore_MongoDB.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace IM_WebAPICore_MongoDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IIncidentService _incidentService;
        private readonly INotificationService _notificationService;
        private readonly IMessageService _messageService;
        private readonly IJWT _jwt;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMemoryCache _memoryCache;
        public UsersController(IUserService userService, IJWT jWT, IWebHostEnvironment webHostEnvironment, IIncidentService incidentService, IMemoryCache memoryCache, INotificationService notificationService, IMessageService messageService)
        {
            _userService = userService;
            _jwt = jWT;
            _webHostEnvironment = webHostEnvironment;
            _incidentService = incidentService;
            _memoryCache = memoryCache;
            _notificationService = notificationService;
            _messageService = messageService;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateAsync(AuthenticateRequest model)
        {
            var response = await _userService.AuthenticateAsync(model);
            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });
            response.Token = _jwt.GenerateToken(response.user, DateTime.UtcNow.AddDays(10));
            
            List<User> allUsers = _memoryCache.Get<List<User>>("allUsers");
            if (allUsers is null)
            {
                allUsers = await _userService.GetAllUsersAsync();
                _memoryCache.Set("allUsers", allUsers);
            }
            return Ok(response);
        }

        [HttpGet("AllUsers")]
        // [Authorize]
        public async Task<IActionResult> AllUsersAsync()
        {           
             var allUsers = await _userService.GetAllUsersAsync();              
             return Ok(allUsers);
        }


        [HttpGet("DataGenerator")]
        public async Task<IActionResult> DataGenerator(string pass)
        {
            if (pass != "umar")
                return BadRequest("Pass is wrong.");

            //DataGenerator data = new DataGenerator();
            //List<User> users = new();
            //var results = data.GeneratePeople().Take(500);
            //foreach (User u in results)
            //{
            //    var user = await _userService.AddUserAsync(u);
            //    await _userService.CreateLoginAsync(user);
            //}
            DataGenerator data = new DataGenerator();
            List<User> users = await _userService.GetAllUsersAsync();
            var results = data.GenerateIncidents(users).Take(100000);

           
            try
            {
                var fileText = System.IO.File.ReadAllText("txtfile.txt");
                List<string> lines = fileText.Split('.').ToList();
                int index = 0;

                foreach (var incident in results)
                {
                    if (lines.Count- 4 < index) 
                        break;

                    string text = lines[index++];
                    incident.Title = text.Length > 100 ? text.Substring(0, 100).Replace("\r", "").Replace("\n", "") : text.Replace("\r", "").Replace("\n", "");
                    text = lines[index++];
                    incident.Description = text.Length > 200 ? text.Substring(0, 200).Replace("\r", "").Replace("\n", "") : text.Replace("\r", "").Replace("\n", "");
                    text = lines[index++];
                    incident.AdditionalData = text.Length > 200 ? text.Substring(0, 200).Replace("\r", "").Replace("\n", "") : text.Replace("\r", "").Replace("\n", "");

                    incident.CreatedAT = incident.StartTime.AddDays(-10);
                    if (incident.DueDate <= incident.StartTime)
                        incident.DueDate = incident.StartTime.AddMonths(10);


                    if (incident.Title.Length < 20)
                        continue;
                    if (incident.Description.Length < 20)
                        continue;

                     // await _incidentService.AddIncident(incident);
                }
            }
            catch(Exception ex)
            {
                return BadRequest("Error." + ex.Message);
            }


            //DataGenerator data = new DataGenerator();
            //List<User> users = await _userService.GetAllUsersAsync();
            //var results = data.GenerateMessages(users.Take(50)).Take(2000);
            //foreach(Message message in results)
            //{
            //    if (message.From == message.To) continue;
            //    await _messageService.AddMessageAsync(message.From, message.To, message.MessageText);
            //}

            return Ok("Data has been generated.");
        }



        [HttpPost("AddUser")]
        [Authorize]
        public async Task<IActionResult> AddUser()
        {
            User user = new User();
            user.FirstName = HttpContext.Request.Form["FirstName"];
            user.LastName = HttpContext.Request.Form["LastName"];
            user.Email = HttpContext.Request.Form["Email"];
            user.Phone = HttpContext.Request.Form["Phone"];
            user.ProfilePic = HttpContext.Request.Form.Files.Count > 0 ? HttpContext.Request.Form.Files[0].FileName : "";

            if (user == null || string.IsNullOrWhiteSpace(user.FirstName)
                 || string.IsNullOrWhiteSpace(user.LastName) || string.IsNullOrWhiteSpace(user.Email)
                )
            {
                return BadRequest(new { message = "Please enter all required fields." });
            }

            var userAdded = await _userService.AddUserAsync(user);
            string userId = userAdded.Id;
            

            if (HttpContext.Request.Form.Files.Count > 0)
            {
                foreach (var formFile in HttpContext.Request.Form.Files)
                {
                    if (formFile.Length > 0)
                    {

                        string folder = _webHostEnvironment.ContentRootPath + "\\Attachments\\Users\\" + userId;
                        Directory.CreateDirectory(folder);
                        string path = folder + "\\" + formFile.FileName;

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await formFile.CopyToAsync(stream);
                        }
                    }
                }

            }//end of if count > 0

            return Ok("New User has been added.");
        }

        [HttpGet("GetUsersWithPage")]
        [Authorize]
        public async Task<IActionResult> GetUsersWithPageAsync(int PageSize = 5, int PageNumber = 1, string SortBy = "a", string SortDirection = "a", string? Search = "")
        {
            var response = await _userService.GetUsersPageAsync(PageSize, PageNumber, SortBy, SortDirection, Search);
            return Ok(response);
        }


        [HttpPost("UpdateHubId")]
        [Authorize]
        public async Task<IActionResult> UpdateHubIdAsync([FromBody] HubUpdate HU)
        {
            var response = await _userService.UpdateHubIdAsync(HU.UserId, HU.HubId);
            if (!response)
                return BadRequest(new { message = "Could not update hubId. Error : "});
            return Ok();
        }

        [HttpGet("UpdateIsRead")]
        [Authorize]
        public async Task<IActionResult> UpdateIsReadAsync(string notificationId, string isRead)
        {
            bool isReadStatus = bool.Parse(isRead);
            var isSuccess = await _notificationService.UpdateIsReadAsync(notificationId, isReadStatus);
            if (!isSuccess)
                return StatusCode(500);
            return Ok();
        }


        [HttpGet("UserNotifications")]
        [Authorize]
        public async Task<IActionResult> UserNotificationsAsync(string userId)
        {
            return Ok(await _notificationService.GetUserNotificationsAsync(userId));
        }


        [HttpGet]
        public async Task<List<User>> Get() =>
         await _userService.GetAsync();

        //[HttpPost]
        //public async Task<IActionResult> Post(User newUser)
        //{
        //    await _userService.CreateAsync(newUser);
        //    var s = CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
        //    return Ok(s);
        //}



       


    }// end of class
}
