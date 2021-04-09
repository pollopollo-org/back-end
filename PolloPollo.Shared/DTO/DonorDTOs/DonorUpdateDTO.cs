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
    }
}