using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static TaskProject.ViewModel.AccountViewModel;

namespace TaskProject.ViewModel
{
    public class AccountDetailsViewModel
    {
        public AccountViewModel Account { get; set; }

        public List<AccountViewModel.TransferHistoryDTO> IncomingMovements { get; set; } = new List<AccountViewModel.TransferHistoryDTO>();

        public List<AccountViewModel.TransferHistoryDTO> OutgoingMovements { get; set; } = new List<AccountViewModel.TransferHistoryDTO>();
    }
}