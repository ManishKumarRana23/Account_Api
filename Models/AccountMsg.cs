using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountMicroservice.Models
{
    public class AccountMsg
    {
        public int accountId { get; set; }
        public string AccType { get; set; }
        public double AccBal { get; set; }
    }
}
