using System.ComponentModel.DataAnnotations;

namespace PolloPollo.Shared.DTO
{
    public class DetailedUserDTO
    {
        public int UserId { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string SurName { get; set; }

        public string UserRole { get; set; }

        public string Country { get; set; }

        public string Description { get; set; }

        public string Thumbnail { get; set; }

        /* Adress for producer */

        // Street only contains characters
        [RegularExpression(@"[^0-9]+")]
        public string Street { get; set; }

        public string StreetNumber { get; set; }

        public string Zipcode { get; set; }

        // City only contains characters
        [RegularExpression(@"[^0-9]+")]
        public string City { get; set; }

        /* Statistics for producer */

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
