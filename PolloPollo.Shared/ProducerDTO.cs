using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;


namespace PolloPollo.Shared
{
    public class ProducerDTO : UserDTO
    {
        public int ProducerId { get; set; }

        public int UserId { get; set; }

        public string Wallet { get; set; }
    }
}
