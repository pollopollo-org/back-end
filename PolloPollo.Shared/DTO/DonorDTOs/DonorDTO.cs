using System;
using System.Collections.Generic;
using System.Text;

namespace PolloPollo.Shared.DTO
{
    public class DonorDTO
    {
        public int Id { get; set; }
        public string AaAccount { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string DeviceAddress { get; set; }
        public string WalletAddress { get; set; }
    }
}
