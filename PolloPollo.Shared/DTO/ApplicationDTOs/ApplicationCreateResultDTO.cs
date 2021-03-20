using System;
using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared.DTO
{
    public class ApplicationCreateResultDTO
    {
        [Required]
        public string UnitId { get; set; }

        [Required]
        public bool Success { get; set; }
                
        public string Details { get; set; }            
    }
}
