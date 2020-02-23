using System;
using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Entities
{
    public partial class Contracts
    {
        [Key]
        public int ApplicationId { get; set; }

        public DateTime? CreationTime { get; set; }

        public int? Completed { get; set; }

        public string ConfirmKey { get; set; }

        public string SharedAddress { get; set; }

        public string DonorWallet { get; set; }

        public string DonorDevice { get; set; }

        public string ProducerWallet { get; set; }

        public string ProducerDevice { get; set; }

        public int? Price { get; set; }

        public int? Bytes { get; set; }
    }
}
