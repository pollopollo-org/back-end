using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared.DTO
{
    public class ProducerCreateDTO : UserCreateDTO
    {
        // Street only contains characters
        [RegularExpression(@"[^0-9]+")]
        [Required]
        public string Street { get; set; }

        [Required]
        public string StreetNumber { get; set; }

        public string ZipCode { get; set; }

        // City only contains characters
        [RegularExpression(@"[^0-9]+")]
        [Required]
        public string City { get; set; }
    }
}
