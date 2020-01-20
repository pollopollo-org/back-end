using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string SurName { get; set; }

        public string UserRole { get; set; }

        public string Country { get; set; }

        public string Description { get; set; }
        
        public string Thumbnail { get; set; }

        // Street only contains characters
        [RegularExpression(@"[^0-9]+")]
        public string Street { get; set; }

        public string StreetNumber { get; set; }

        public string Zipcode { get; set; }

        // City only contains characters
        [RegularExpression(@"[^0-9]+")]
        public string City { get; set; }
    }
}
