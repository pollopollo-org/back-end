using System;
using System.ComponentModel.DataAnnotations;
using PolloPollo.Shared;

namespace PolloPollo.Entities
{
    public class Application
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public User User { get; set; }

        [Required]
        public int ProductId { get; set; }

        public Product Product { get; set; }

        [StringLength(255)]
        public string Motivation { get; set; }

        [Required]
        public DateTime TimeStamp { get; set;  }

        [Required]
        public ApplicationStatus Status { get; set; }
    }
}