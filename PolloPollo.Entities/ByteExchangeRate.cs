using System;
using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public partial class ByteExchangeRate
    {
        [Key]
        public int Id { get; set; }

        public decimal GBYTE_USD { get; set; }
    }
}
