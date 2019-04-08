using System;

namespace PolloPollo.Shared.DTO
{
    public class ApplicationDTO
    {
        public int ApplicationId { get; set; }

        public int UserId { get; set; }

        public int ProductId { get; set; }

        public string Motivation { get; set; }

        public DateTime TimeStamp { get; set; }

        public ApplicationStatus Status { get; set; }
    }
}
