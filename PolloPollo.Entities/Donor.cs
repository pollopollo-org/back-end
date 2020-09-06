using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public partial class Donor
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(32)]
        public string UID { get; set; }

        [StringLength(64)]
        [Required]
        public string Password { get; set; }

        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }

        [MaxLength(34)]
        public string DeviceAddress { get; set; }

        [MaxLength(34)]
        public string WalletAddress { get; set; }

        [MaxLength(128)]
        [Required]
        public string AaAccount { get; set; }
    }
}
