
using BogusData.Data;
using IM.Models;
using IM_DataAccess.DataService;
using IM_DataAccess.Models;
using IM_WebAPICore_MongoDB.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IM_WebAPICore_MongoDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IJWT _jwt;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public UsersController(IUserService userService, IJWT jWT, IWebHostEnvironment webHostEnvironment)
        {
            _userService = userService;
            _jwt = jWT;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateAsync(AuthenticateRequest model)
        {
            var response = await _userService.AuthenticateAsync(model);
            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });
            response.Token = _jwt.GenerateToken(response.user, DateTime.UtcNow.AddDays(10));
            return Ok(response);
        }

        [HttpGet("AllUsers")]
        // [Authorize]
        public async Task<IActionResult> AllUsersAsync()
        {
            //DataGenerator data = new DataGenerator();
            //List<User> users = new();
            //var results = data.GeneratePeople().Take(500);
            //foreach (User u in results)
            //{
            //    var user = await _userService.AddUserAsync(u);
            //    await _userService.CreateLoginAsync(user);
            //}
            return Ok(await _userService.GetAllUsersAsync());
        }

        [HttpPost("AddUser")]
        //[Authorize]
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
        //[Authorize]
        public async Task<IActionResult> GetUsersWithPageAsync(int PageSize = 5, int PageNumber = 1, string SortBy = "a", string SortDirection = "a", string? Search = "")
        {
            var response = await _userService.GetUsersPageAsync(PageSize, PageNumber, SortBy, SortDirection, Search);
            return Ok(response);
        }


        [HttpPost("UpdateHubId")]
       // [Authorize]
        public async Task<IActionResult> UpdateHubIdAsync([FromBody] HubUpdate HU)
        {
            var response = await _userService.UpdateHubIdAsync(HU.UserId, HU.HubId);
            if (!response)
                return BadRequest(new { message = "Could not update hubId. Error : "});
            return Ok();
        }

        [HttpGet]
        public async Task<List<User>> Get() =>
         await _userService.GetAsync();

        [HttpPost]
        public async Task<IActionResult> Post(User newUser)
        {
            await _userService.CreateAsync(newUser);

            var s = CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
            return Ok(s);
        }
    }// end of class
}
