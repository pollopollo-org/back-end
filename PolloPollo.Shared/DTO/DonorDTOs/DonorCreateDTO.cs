using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
namespace PolloPollo.Shared.DTO
{
    public class DonorCreateDTO
    {

        [Required]
        [StringLength(128)]
        public string AaAccount { get; set; }

        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [StringLength(64)]
        [MinLength(8)]
        public string Password { get; set; }
    }
}
