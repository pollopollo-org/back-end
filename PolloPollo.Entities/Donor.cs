using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public partial class Donor
    {
        [Key]
        public int Id { get; set; }

        [StringLength(32)]
        public string UID { get; set; }

        [Required]
        [StringLength(64)]
        [MinLength(8)]
        public string Password { get; set; }

        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [StringLength(34)]
        public string DeviceAddress { get; set; }

        [StringLength(34)]
        public string WalletAddress { get; set; }

        [Required]
        [StringLength(128)]        
        public string AaAccount { get; set; }
    }
}
