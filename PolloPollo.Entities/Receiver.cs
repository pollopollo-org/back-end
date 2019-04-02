using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public partial class Receiver
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public User User { get; set; }
    }
}
