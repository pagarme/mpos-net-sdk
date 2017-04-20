using System;
using System.Threading.Tasks;
using PagarMe.Mpos.Bridge.Commands;

namespace PagarMe.Mpos.Bridge.Providers
{
    public interface IProvider : IDisposable
    {
        Task Open(InitializationOptions options);
        Task SynchronizeTables(bool force);
        Task DisplayMessage(string message);
        Task<ProcessPaymentResponse> ProcessPayment(ProcessPaymentRequest request);
        Task FinishPayment(FinishPaymentRequest request);
        Task CancelOperation();
        Task Close();
    }
}