using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public partial class Producer
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        public string Wallet { get; set; }

        [Required]
        public string PairingCode { get; set; }

        [Required]
        public int UserId { get; set; }

        public User User { get; set; }

    }
}
