using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataTransferObjects
{
    public class MessagesListRequestData
    {
    }

    public class MessagesListResponseData
    {
        [JsonProperty("messages")] public List<SendMessageResponseData> Messages { get; set; }
    }

    public class LoginRequestData
    {
        [JsonRequired]
        [JsonProperty("username")]
        public string Username { get; set; }
    }

    public class LoginResponseData
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("username")] public string Username { get; set; }

        [JsonProperty("online")] public bool Online { get; set; }

        [JsonProperty("token")] public Guid Token { get; set; }
    }

    public class LogoutRequestData
    {
    }

    public class LogoutResponseData
    {
        [JsonProperty("message")] public string Message { get; set; }
    }

    public class SendMessageRequestData
    {
        [JsonRequired]
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class SendMessageResponseData
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("message")] public string Message { get; set; }

        [JsonProperty("author")] public int Author { get; set; }
    }

    public class UserInfoRequestData
    {
    }

    public class UserInfoResponseData
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("username")] public string Username { get; set; }

        [JsonProperty("online")] public bool? Online { get; set; }
    }

    public class UserListRequestData
    {
    }

    public class UserListResponseData
    {
        [JsonProperty("users")] public List<UserInfoResponseData> Users { get; set; }
    }
}
