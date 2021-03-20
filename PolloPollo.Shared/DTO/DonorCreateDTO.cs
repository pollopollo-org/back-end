﻿using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
namespace PolloPollo.Shared.DTO
{
    public class DonorCreateDTO
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string AaAccount { get; set; }

        [StringLength(32)]
        public string UID { get; set; }
        
        [StringLength(64)]
        [MinLength(8)]
        public string Password { get; set; }

        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [StringLength(34)]
        public string DeviceAddress { get; set; }

        [Required]
        [StringLength(34)]
        public string WalletAddress { get; set; }
    }
}
