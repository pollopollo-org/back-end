using System;

namespace PolloPollo.Shared.DTO
{
    public class ApplicationDTO
    {
        public int ApplicationId { get; set; }

        public int ReceiverId { get; set; }

        public string ReceiverName { get; set; }

        public string Country { get; set; }

        public string Thumbnail { get; set; }

        public int ProductId { get; set; }

        public string ProductTitle { get; set; }

        public int ProductPrice { get; set; }

        public int ProducerId { get; set; }

        public string Motivation { get; set; }

        public int Bytes { get; set; }


        public ApplicationStatusEnum Status { get; set; }

        public string DateOfDonation { get; set; }

        public string CreationDate { get; set; }
    }
}
