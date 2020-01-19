namespace PolloPollo.Shared.DTO
{
    public class DetailedProducerDTO : DetailedUserDTO
    {
        public string Wallet { get; set; }

        public string PairingLink { get; set; }

        // Statistics for producer

        public int CompletedDonationsPastWeekNo { get; set; }

        public int CompletedDonationsPastMonthNo { get; set; }

        public int CompletedDonationsAllTimeNo { get; set; }

        public int CompletedDonationsPastWeekPrice { get; set; }

        public int CompletedDonationsPastMonthPrice { get; set; }

        public int CompletedDonationsAllTimePrice { get; set; }

        public int PendingDonationsPastWeekNo { get; set; }
 
        public int PendingDonationsPastMonthNo { get; set; }

        public int PendingDonationsAllTimeNo { get; set; }

        public int PendingDonationsPastWeekPrice { get; set; }

        public int PendingDonationsPastMonthPrice { get; set; }

        public int PendingDonationsAllTimePrice { get; set; }
    }
}
