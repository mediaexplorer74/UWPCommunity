﻿using Newtonsoft.Json;

namespace UWPCommLib.Api.Discord.Models
{
    public class LoginRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("captcha_key")]
        public string[] CaptchaKey { get; set; }
    }
    public class SendSmsRequest
    {
        [JsonProperty("ticket")]
        public string Ticket { get; set; }
    }
    public class LoginMFARequest
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("ticket")]
        public string Ticket { get; set; }
    }
}
