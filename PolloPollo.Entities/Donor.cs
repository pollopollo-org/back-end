using System.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public partial class Donor
    {
        [Key]      
        [Required]
        [StringLength(128)]
        public string AaAccount { get; set; }

        [StringLength(128)]
        public string UID { get; set; }
        
        [StringLength(256)]
        [MinLength(8)]
        public string Password { get; set; }

        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [StringLength(34)]
        public string DeviceAddress { get; set; }

        [StringLength(34)]
        public string WalletAddress { get; set; }

        public string Thumbnail { get; set; }
    }
}
