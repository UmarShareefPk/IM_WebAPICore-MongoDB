using IM_DataAccess.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IM_DataAccess.DataService
{
    public interface IMessageService
    {
        Task<object> AddMessageAsync(string From, string To, string MessageText);
        Task<object> ChangeMessageStatusAsync(string MessageId, string Status);
        Task<bool> DeleteConversationAsync(string ConversationId);
        Task<object> DeleteMessageAsync(string MessageId);
        Task<List<Conversation>> GetConversationsByUserAsync(string UserId);
        Task<List<Message>> GetMessagesByConversationsAsync(string ConversationId);
        Task<object> GetMessagesByUserAsync(string UserId);
    }

    public class MessageService : IMessageService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Conversation> _conversationCollection;
        private readonly IMongoCollection<Message> _messageCollection;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _memoryCache;

        public MessageService(IConfiguration config, IMemoryCache memoryCache)
        {
            _config = config;
            MongoClient client = new MongoClient(_config.GetValue<string>("MongoDbCon"));
            IMongoDatabase database = client.GetDatabase(_config.GetValue<string>("DB"));
            _userCollection = database.GetCollection<User>("Users");
            _conversationCollection = database.GetCollection<Conversation>("Conversations");
            _messageCollection = database.GetCollection<Message>("Messages");
            _memoryCache = memoryCache;
        }

        public async Task<object> AddMessageAsync(string From, string To, string MessageText)
        {
            Conversation conversation = null;
            //check if new conversation needed

            conversation = await _conversationCollection.Find(c => (c.User1 == From && c.User2 == To) || (c.User1 == To && c.User2 == From)).FirstOrDefaultAsync();
            if (conversation is not null)
            {
                var filter = Builders<Conversation>.Filter
               .Eq(c => c.Id, conversation.Id);
                var update = Builders<Conversation>.Update
                    .Set(c => c.LastMessage, MessageText)
                    .Set(c => c.LastMessageTime, DateTime.UtcNow);

                var updateResult = await _conversationCollection.UpdateOneAsync(filter, update);
            }
            else
            {
                Conversation newConversation = new Conversation
                {
                    User1 = From,
                    User2 = To,
                    LastMessage = MessageText,
                    LastMessageTime = DateTime.UtcNow
                };
                await _conversationCollection.InsertOneAsync(newConversation);
                conversation = newConversation;
            }

            Message newMessage = new Message
            {
                From = From,
                To = To,
                MessageText = MessageText,
                Status = "Unread",
                Deleted = false,
                ConversationId = conversation.Id,
            };

            await _messageCollection.InsertOneAsync(newMessage);

            return new List<object> { newMessage, conversation };
        }


        public async Task<List<Conversation>> GetConversationsByUserAsync(string UserId)
        {
            var conversations = await _conversationCollection.Find(c => c.User1 == UserId || c.User2 == UserId).ToListAsync();

            conversations.Select(c =>
            {
                c.UnReadCount = (int)_messageCollection.Find(m => m.To == UserId || m.ConversationId == c.Id).CountDocuments();
                return c;
            });
            return conversations;
        }

        public async Task<bool> DeleteConversationAsync(string ConversationId)
        {
            await _messageCollection.DeleteManyAsync(m => m.ConversationId == ConversationId);
            await _conversationCollection.DeleteOneAsync(c => c.Id == ConversationId);
            return true;
        }

        public async Task<object> DeleteMessageAsync(string MessageId)
        {
            var message = await _messageCollection.Find(m => m.Id == MessageId).FirstOrDefaultAsync();
            var conversationId = message.ConversationId;

            await _messageCollection.DeleteOneAsync(m => m.Id == MessageId);

            var messages = await _messageCollection.Find(m => m.ConversationId == conversationId).ToListAsync();
            var lastMessage = messages.OrderByDescending(m => m.Date).First();

            var filter = Builders<Conversation>.Filter
             .Eq(c => c.Id, conversationId);
            var update = Builders<Conversation>.Update
                .Set(c => c.LastMessage, lastMessage.MessageText)
                .Set(c => c.LastMessageTime, lastMessage.Date);

            return true;
        }

        public async Task<object> ChangeMessageStatusAsync(string MessageId, string Status)
        {
            var filter = Builders<Message>.Filter
           .Eq(m => m.Id, MessageId);
            var update = Builders<Message>.Update
                .Set(m => m.Status, "read");

            var updateResult = await _messageCollection.UpdateOneAsync(filter, update);

            if (updateResult.ModifiedCount > 0)
                return true;

            return false;
        }

        public async Task<List<Message>> GetMessagesByConversationsAsync(string ConversationId)
        {
            return await _messageCollection.Find(m => m.ConversationId == ConversationId).ToListAsync();
        }

        public async Task<object> GetMessagesByUserAsync(string UserId)
        {
            return null;
        }



    }
}
