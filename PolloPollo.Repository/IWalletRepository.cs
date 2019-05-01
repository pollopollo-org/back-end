using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PolloPollo.Services
{
    public interface IWalletRepository
    {
        Task<bool> ConfirmReceival(int ApplicationId);
    }
}
