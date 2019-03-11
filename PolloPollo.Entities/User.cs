using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public partial class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Description { get; set; }


    }
}
