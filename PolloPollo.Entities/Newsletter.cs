using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public partial class Newsletter
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(64)]        
        public string DeviceAddress { get; set; }
    }
}
