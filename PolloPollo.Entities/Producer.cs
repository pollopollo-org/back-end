using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public partial class Producer
    {
        [Key]
        public int Id { get; set; }

        public string WalletAddress { get; set; }

        [Required]
        [StringLength(255)]
        public string PairingSecret { get; set; }

        public string DeviceAddress { get; set; }

        [Required]
        public int UserId { get; set; }

        public User User { get; set; }

    }
}
