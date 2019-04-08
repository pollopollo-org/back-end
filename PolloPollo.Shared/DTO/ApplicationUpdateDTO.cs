using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared.DTO
{
    public class ApplicationUpdateDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public ApplicationStatus Status { get; set; }
    }
}
