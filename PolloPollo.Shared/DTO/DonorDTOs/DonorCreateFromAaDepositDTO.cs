using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared.DTO
{
    public class DonorFromAaDepositDTO
    {
        [Required]
        public string AccountId { get; set; }

        [Required]
        public string WalletAddress { get; set; }
    }
}
