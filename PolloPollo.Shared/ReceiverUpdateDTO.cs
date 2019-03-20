using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations; 

namespace PolloPollo.Shared
{
    public class ReceiverUpdateDTO : UserUpdateDTO
    {
        public int ReceiverId { get; set; }
    }
}
