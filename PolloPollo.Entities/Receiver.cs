using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public partial class Receiver
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public User User { get; set; }

    }
}
