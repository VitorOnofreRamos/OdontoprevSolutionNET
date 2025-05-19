using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Auth.API.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Username")]
        public string Username { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("CPF")]
        public string CPF { get; set; }

        [BsonElement("PasswordHash")]
        public byte[] PasswordHash { get; set; }

        [BsonElement("PasswordSalt")]
        public byte[] PasswordSalt { get; set; }

        [BsonElement("Phone")]
        public string Phone { get; set; }

        [BsonElement("Role")]
        public string Role { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("LastLogin")]
        public DateTime? LastLogin { get; set; }

        [BsonElement("Active")]
        public bool Active { get; set; }
    }
}
