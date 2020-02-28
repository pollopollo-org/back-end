using System;
namespace PolloPollo.Shared
{
    public static class BytesToUSDConverter
    {
        public static decimal BytesToUSD(int bytes, decimal exchangeRate)
        {
            if (bytes == 0) {
                return 0M;
            }
            var GB = 1_000_000_000;
            decimal USD = ((decimal)bytes / GB) * exchangeRate;

            return Math.Round(USD, 2);
        }
    }
}
