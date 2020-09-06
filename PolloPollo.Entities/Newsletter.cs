using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public partial class Newsletter
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(64)]
        [Required]
        public string DeviceAddress { get; set; }
    }
}
