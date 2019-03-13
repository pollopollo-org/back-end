using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PolloPollo.Shared
{
    public class ReceiverDTO : UserDTO
    {
        public int ReceiverId { get; set; }

        public int UserId { get; set; }
    }
}
