using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations; 

namespace PolloPollo.Shared
{
    public class ProducerUpdateDTO : UserUpdateDTO
    {
        public int ProducerId { get; set; }

        public string Wallet { get; set; }
    }
}
