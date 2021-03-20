using System;
using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared.DTO
{
    public class ApplicationUpdateDTO
    {
        [Required]
        public int ApplicationId { get; set; }
       
        public int ReceiverId { get; set; }

        public string UnitId { get; set; }

        [Required]
        public ApplicationStatusEnum Status { get; set; }
            
    }
}
