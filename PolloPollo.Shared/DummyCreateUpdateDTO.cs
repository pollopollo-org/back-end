using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared
{
    public class DummyCreateUpdateDTO
    {
        [Required]
        public string Description { get; set; }
    }
}
