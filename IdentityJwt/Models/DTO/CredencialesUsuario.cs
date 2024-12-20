﻿using System.ComponentModel.DataAnnotations;

namespace IdentityJwt.Models.DTO
{
    public class CredencialesUsuario
    {

        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
