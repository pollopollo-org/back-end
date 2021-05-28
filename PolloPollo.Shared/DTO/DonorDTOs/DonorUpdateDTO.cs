using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
namespace PolloPollo.Shared.DTO
{
    public class DonorUpdateDTO : DonorCreateDTO
    {
        [Required]
        [StringLength(128)]
        public string AaAccount { get; set; }
        [StringLength(34)]
        public string DeviceAddress { get; set; }

        [StringLength(34)]
        public string WalletAddress { get; set; }
        [StringLength(255)]
        public string FirstName { get; set; }

        [StringLength(255)]
        public string SurName { get; set; }
        [StringLength(255)]
        public string Country { get; set; }

        public string Thumbnail { get; set; }
    }
}