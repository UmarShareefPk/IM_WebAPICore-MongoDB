
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
using System.Net;

namespace IM_WebAPICore_MongoDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly INotificationService _notificationService;
       
        private readonly IMemoryCache _memoryCache;
        public MessagesController(IUserService userService,INotificationService notificationService, IMessageService messageService)
        {
            _userService = userService;  
            _notificationService = notificationService;
            _messageService = messageService;
        }

        [HttpPost("AddMessage")]
        [Authorize]
        public async Task<IActionResult> AddMessage()
        {

            string From = HttpContext.Request.Form["From"];
            string To = HttpContext.Request.Form["To"];
            string MessageText = HttpContext.Request.Form["MessageText"];

            if (string.IsNullOrWhiteSpace(From) || string.IsNullOrWhiteSpace(To) || string.IsNullOrWhiteSpace(MessageText)
                )
            {
                return BadRequest(new { message = "Please enter all required fields." });
            }
            var messages = await _messageService.AddMessageAsync(From, To, MessageText);          

            return Ok(messages);
        }



        [HttpGet("ConversationsByUser")]
        [Authorize]
        public async Task<IActionResult> GetConversationsByUser(string UserId)
        {
            var result = await _messageService.GetConversationsByUserAsync(UserId);
            return Ok(result);
        }

        [HttpPost("DeleteConversation")]
        [Authorize]
        public async Task<IActionResult> DeleteConversation(string ConversationId)
        {
            var isSuccess = await _messageService.DeleteConversationAsync(ConversationId);

            if (!isSuccess) 
               return StatusCode(500, new { message = "Error in deleting conversation" });            

            return Ok();
        }

        [HttpPost("DeleteMessage")]
        [Authorize]
        public async Task<IActionResult> DeletMessage([FromQuery] string MessageId)
        {
            var isSuccess = await _messageService.DeleteMessageAsync(MessageId);

            if (!isSuccess)
                return StatusCode(500, new { message = "Error in deleting message" });           

            return Ok();
        }

        [HttpPost("ChangeMessageStatus")]
        [Authorize]
        public async Task<IActionResult> ChangeMessageStatus(string MessageId, string Status)
        {
            var isSuccess = await _messageService.ChangeMessageStatusAsync(MessageId, Status);

            if (!isSuccess)
                return StatusCode(500, new { message = "Error in deleting message" });

            return Ok();
        }


        [HttpGet("MessagesByConversations")]
        [Authorize]
        public async Task<IActionResult> GetMessagesByConversations(string ConversationId)
        {
            var messages = await _messageService.GetMessagesByConversationsAsync(ConversationId);
            return Ok(messages);
        }

        [HttpGet("MessagesByUser")]
        [Authorize]
        public async Task<IActionResult> GetMessagesByUser(string UserId)
        {           

            return Ok();
        }





    }// end of class
}
