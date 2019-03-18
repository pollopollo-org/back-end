using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using PolloPollo.Shared;

namespace PolloPollo.Entities
{
    public class UserRole
    {
        [Key]
        public int UserId { get; set; }

        [Key]
        public UserRoleEnum UserRoleEnum { get; set; }
    }
}
