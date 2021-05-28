using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
namespace PolloPollo.Shared.DTO
{
    public class DonorListDTO
    {
        public string AaAccount { get; set; }
        public string Email { get; set; }
        public string UID { get; set; }
    }
}