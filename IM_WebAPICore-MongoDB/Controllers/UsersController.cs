
using IM.Models;
using IM_DataAccess.DataService;
using IM_DataAccess.Models;
using IM_WebAPICore_MongoDB.Utilities;
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
        public UsersController(IUserService userService, IJWT jWT)
        {
            _userService = userService;
            _jwt = jWT;
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

        [HttpGet]
        public async Task<List<User>> Get() =>
         await _userService.GetAsync();

        [HttpPost]
        public async Task<IActionResult> Post(User newUser)
        {
            await _userService.CreateAsync(newUser);

            return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
        }
    }// end of class
}
