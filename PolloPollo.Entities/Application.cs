using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        [Required]
        [Column(TypeName = "text")]
        public string Motivation { get; set; }

        [Required]
        public DateTime Created { get; set;  }

        [Required]
        public DateTime LastModified { get; set; }

        public DateTime DonationDate { get; set; }

        [Required]
        public ApplicationStatusEnum Status { get; set; }
    }
}
