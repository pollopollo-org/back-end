using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PolloPollo.Shared
{
    public class AuthenticateDTO
    {
        [EmailAddress]
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
