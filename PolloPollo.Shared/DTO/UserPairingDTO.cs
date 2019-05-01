using System;
namespace PolloPollo.Shared.DTO
{
    public class UserPairingDTO
    {
        public string PairingSecret { get; set; }
        public string DeviceAddress { get; set; }
        public string WalletAddress { get; set; }
    }
}
