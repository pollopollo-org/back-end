namespace PolloPollo.Shared.DTO
{
    public class DetailedProducerDTO : DetailedUserDTO
    {
        public string Wallet { get; set; }

        public string PairingLink { get; set; }

        public string Street { get; set; }

        public string StreetNumber { get; set; }

        public string Zipcode { get; set; }

        public string City { get; set; }
    }
}
