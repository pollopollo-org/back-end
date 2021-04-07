using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
namespace PolloPollo.Shared.DTO
{
    public class DonorUpdateDTO : DonorCreateDTO
    {
        [StringLength(34)]
        public string DeviceAddress { get; set; }

        [StringLength(34)]
        public string WalletAddress { get; set; }
    }
}