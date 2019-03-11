using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public partial class DummyEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public User User { get; set; }

    }
}
