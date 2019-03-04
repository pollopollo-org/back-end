using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared
{
    public class DummyCreateUpdateDTO
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
