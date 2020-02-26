using System;
namespace PolloPollo.Services.Utils
{
    public class BytesToUSDConverter
    {
        public BytesToUSDConverter()
        {
        }

        public static double? BytesToUSD(int? bytes, decimal exchangeRate)
        {
            if (bytes == null) {
                return null;
            }
            var GB = 1000000000;
            var USD = (double)(bytes * GB / exchangeRate);

            return Math.Round(USD, 2);
        }
    }
}
