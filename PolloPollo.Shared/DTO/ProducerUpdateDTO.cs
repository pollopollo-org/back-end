using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared.DTO
{
    public class ProducerUpdateDTO : UserUpdateDTO
    {
        [StringLength(255)]
        public string Wallet { get; set; }

        // Street only contains characters
        [RegularExpression(@"[^0-9]+")]
        public string Street { get; set; }

        public string StreetNumber { get; set; }

        public string ZipCode { get; set; }

        // City only contains characters
        [RegularExpression(@"[^0-9]+")]
        [StringLength(255)]
        public string City { get; set; }
    }
}