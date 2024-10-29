using System.Collections.Generic;

namespace TaskProject.ViewModel
{
    public class TransferHistoryCombinedViewModel
    {
        public IEnumerable<TransferHistoryViewModel> IncomingTransfers { get; set; }
        public IEnumerable<TransferHistoryViewModel> OutgoingTransfers { get; set; }
    }
}
