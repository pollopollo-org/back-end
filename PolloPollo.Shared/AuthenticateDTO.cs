using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PolloPollo.Shared
{
    class AuthenticateDTO
    {
        public int Id { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public int Password { get; set; }
    }
}
