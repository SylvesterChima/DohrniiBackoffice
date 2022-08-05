﻿namespace DohrniiBackoffice.DTO.Request
{
    public class RegisterRequest
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
