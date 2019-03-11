using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public partial class Producer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public User User { get; set; }

        public string Wallet { get; set; }
    }
}
