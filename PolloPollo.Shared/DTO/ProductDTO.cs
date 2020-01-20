using System.Collections.Generic;

namespace PolloPollo.Shared.DTO
{
    public class ProductDTO
    {
        public int ProductId { get; set; }

        public string Title { get; set; }

        public int UserId { get; set; }

        public int Price { get; set; }

        public string Description { get; set; }

        public string Country { get; set; }

        public string Location { get; set; }

        public bool Available { get; set; }

        public string Thumbnail { get; set; }

        public int Rank { get; set; }

        public IEnumerable<ApplicationDTO> OpenApplications { get; set; }

        public IEnumerable<ApplicationDTO> PendingApplications { get; set; }

        public IEnumerable<ApplicationDTO> ClosedApplications { get; set; }

        /* Product stats */

        public int DateLastDonation { get; set; }

        public int CompletedDonationsPastWeek { get; set; }

        public int CompletedDonationsPastMonth { get; set; }

        public int CompletedDonationsAllTime { get; set; }

        public int PendingDonationsPastWeek { get; set; }

        public int PendingDonationsPastMonth { get; set; }

        public int PendingDonationsAllTime { get; set; }

    }
}
