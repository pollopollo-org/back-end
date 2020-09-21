using System;
using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared.DTO
{
    public class ApplicationCreateDTO
    {

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Motivation { get; set; }

        [Required]
        public int ProducerId { get; set; }
    }
}
