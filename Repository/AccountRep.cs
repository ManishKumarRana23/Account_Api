using AccountMicroservice.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AccountMicroservice.Repository
{
    public class AccountRep : IAccountRep
    {
        int acid = 1;
          Uri baseAddress = new Uri("https://localhost:44334/api");   //Port No.

        HttpClient client;
        public AccountRep()
        {
            client = new HttpClient();
            client.BaseAddress = baseAddress;
        }
        public static List<accountStatement> accountStatements = new List<accountStatement>()
        {
            new accountStatement{accountId=202,
            statements= new List<statement>()
            {
                new statement{date=21092020,narration="Withdrawn",refNumber=12345,valueDate=01022020,withdrawal=1000.00,deposit=0.00,closingBalance=1000.00},
                new statement{date=27092020,narration="Deposited",refNumber=21345,valueDate=04022020,withdrawal=0.00,deposit=2000.00,closingBalance=3000.00}
                }
            }
         };
        public static List<customerAccount> customeraccounts = new List<customerAccount>()
        {
            new customerAccount{customerId=2,currentAccountId=201,savingsAccountId=202},
           //New line added
            new customerAccount{customerId=1,currentAccountId=202,savingsAccountId=203}
        };
        public static List<CurrentAccount> currentAccounts = new List<CurrentAccount>()
        {
            new CurrentAccount{CAId=201,CBal=1000},
             new CurrentAccount{CAId=202,CBal=2000}
        };
        public static List<savingsAccount> savingsAccounts = new List<savingsAccount>()
        {
            new savingsAccount{savingsAccountId=202,savingsAccountbalance=500},
             new savingsAccount{savingsAccountId=203,savingsAccountbalance=2000}
        };
        public List<CurrentAccount> GetCurrent()
        {
            return currentAccounts;
        }

        public List<savingsAccount> GetSavings()
        {
            return savingsAccounts;
        }

        public List<AccountMsg> getCustomerAccounts(int id)
        {
            var a = customeraccounts.Find(c => c.customerId == id);
            var ca = currentAccounts.Find(cac => cac.CAId == a.customerId);
            var sa = savingsAccounts.Find(sac => sac.savingsAccountId == a.savingsAccountId);
            var ac = new List<AccountMsg>
            {
                new AccountMsg{accountId=ca.CAId,AccType="Current Account",AccBal=ca.CBal},
                new AccountMsg{accountId=sa.savingsAccountId,AccType="Savings Account",AccBal=sa.savingsAccountbalance}
            };
            return ac;
        }
        public customerAccount createAccount(int id)
        {
            customerAccount a = new customerAccount
            {
                customerId = id,
                currentAccountId = (id * 100) + acid,
                savingsAccountId = (id * 100) + (acid + 1)
            };
            customeraccounts.Add(a);
            var cust = customeraccounts.Find(c => c.customerId == id);
            CurrentAccount ca = new CurrentAccount
            {
                CAId = cust.currentAccountId,
                CBal = 0.00
            };
            currentAccounts.Add(ca);
            savingsAccount sa = new savingsAccount
            {
                savingsAccountId = cust.savingsAccountId,
                savingsAccountbalance = (int)0.00
            };
            savingsAccounts.Add(sa);
            return cust;
        }

        public AccountMsg getAccount(int id)
        {
            if (id % 2 != 0)
            {
                var ca = currentAccounts.Find(a => a.CAId == id);
                var ac1 = new AccountMsg
                {
                    accountId = ca.CAId,
                    AccType = "Current Account",
                    AccBal = ca.CBal
                };
                return ac1;
            }
            var sa = savingsAccounts.Find(a => a.savingsAccountId == id);
            var ac = new AccountMsg
            {
                accountId = sa.savingsAccountId,
                AccType = "Savings Account",
                AccBal = sa.savingsAccountbalance
            };
            return ac;
        }

        public IEnumerable<statement> getAccountStatement(int AccountId, int from_date, int to_date)
        {
            if (from_date != 0 || to_date != 0)
            {
                var accs = accountStatements.Find(a => a.accountId == AccountId);
                var s = accs.statements;
                foreach (var n in s)
                {
                    if (n.date >= from_date && n.date <= to_date)
                    {
                        return s;
                    }
                }
            }
            var accs1 = accountStatements.Find(a => a.accountId == AccountId);
            var s1 = accs1.statements;
            foreach (var n in s1)
            {
                if (n.date > 01092020 && n.date < 30092020)
                {
                    return s1;
                }
            }
            return null;
        }

        public AccountMsg deposit(dwacc value)
        {
            string data = JsonConvert.SerializeObject(value);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(client.BaseAddress + "/Transaction/deposit/", content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data1 = response.Content.ReadAsStringAsync().Result;
                Ts s1 = JsonConvert.DeserializeObject<Ts>(data1);
                if (s1.Message == "Success")
                {
                    if (value.AccountId % 2 == 0)
                    {
                        var sa = savingsAccounts.Find(a => a.savingsAccountId == value.AccountId);
                        sa.savingsAccountbalance = sa.savingsAccountbalance + value.Balance;
                        AccountMsg sob = new AccountMsg
                        {
                            accountId = value.AccountId,
                            AccType = "Deposited Correctly",
                            AccBal = sa.savingsAccountbalance
                        };
                        return sob;
                    }
                    var ca = currentAccounts.Find(a => a.CAId == value.AccountId);
                    ca.CBal = ca.CBal + value.Balance;
                    AccountMsg cob = new AccountMsg
                    {
                        accountId = value.AccountId,
                        AccType = "Deposited Correctly",
                        AccBal = ca.CBal
                    };
                    return cob;
                }
               /* if (value.AccountId % 2 == 0)
                {
                    var sad = savingsAccounts.Find(a => a.savingsAccountId == value.AccountId);
                    sad.savingsAccountbalance = sad.savingsAccountbalance + value.Balance;
                    AccountMsg sobd = new AccountMsg
                    {
                        accountId = value.AccountId,
                        AccType = "Deposited Correctly",
                        AccBal = sad.savingsAccountbalance
                    };
                    return sobd;
                }
                var cad = currentAccounts.Find(a => a.CAId == value.AccountId);
                cad.CBal = cad.CBal + value.Balance;
                AccountMsg cobd = new AccountMsg
                {
                    accountId = value.AccountId,
                    AccType = "Deposited Correctly",
                    AccBal = cad.CBal
                };
                return cobd;*/
            }
            return null;
        }

        public AccountMsg withdraw(dwacc value)
        {
            string data = JsonConvert.SerializeObject(value);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(client.BaseAddress + "/Transaction/withdraw/", content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data1 = response.Content.ReadAsStringAsync().Result;
                Ts s1 = JsonConvert.DeserializeObject<Ts>(data1);
                AccountMsg amsg = new AccountMsg();
                if (s1.Message == "No Warning")
                {
                    if (value.AccountId % 2 == 0)
                    {
                        var sa = savingsAccounts.Find(a => a.savingsAccountId == value.AccountId);
                        sa.savingsAccountbalance = sa.savingsAccountbalance - value.Balance;
                        if (sa.savingsAccountbalance >= 0)
                        {
                            amsg.accountId = value.AccountId;
                            amsg.AccType = "Withdrawn Successfully";
                            amsg.AccBal = sa.savingsAccountbalance;
                            return amsg;
                        }
                        else
                        {
                            sa.savingsAccountbalance = sa.savingsAccountbalance + value.Balance;
                            amsg.accountId = value.AccountId;
                            amsg.AccType = "Insufficient Fund";
                            amsg.AccBal = sa.savingsAccountbalance;
                            return amsg;
                        }
                    }
                    var car = currentAccounts.Find(a => a.CAId == value.AccountId);
                    car.CBal = car.CBal - value.Balance;
                    if (car.CBal >= 0)
                    {
                        amsg.accountId = value.AccountId;
                        amsg.AccType = "Withdrawn Successfully";
                        amsg.AccBal = car.CBal;
                        return amsg;
                    } 
                    else
                    {
                        car.CBal = car.CBal + value.Balance;
                        amsg.accountId = value.AccountId;
                        amsg.AccType = "Insufficient Fund";
                        amsg.AccBal = car.CBal;
                        return amsg;
                    }

                }
                if (value.AccountId % 2 == 0)
                {
                    var sa = savingsAccounts.Find(a => a.savingsAccountId == value.AccountId);
                    sa.savingsAccountbalance = sa.savingsAccountbalance - value.Balance;
                    if (sa.savingsAccountbalance >= 0)
                    {
                        amsg.accountId = value.AccountId;
                        amsg.AccType = "Withdrawn Successfully.Service charge applicable at the end of month";
                        amsg.AccBal = sa.savingsAccountbalance;
                        return amsg;
                    }
                    else
                    {
                        sa.savingsAccountbalance = sa.savingsAccountbalance + value.Balance;
                        amsg.accountId = value.AccountId;
                        amsg.AccType = "Insufficient Fund";
                        amsg.AccBal = sa.savingsAccountbalance;
                        return amsg;
                    }
                }
                var ca = currentAccounts.Find(a => a.CAId == value.AccountId);
                ca.CBal = ca.CBal - value.Balance;
                if (ca.CBal >= 0)
                {
                    amsg.accountId = value.AccountId;
                    amsg.AccType = "Withdrawn Successfully.Service Charge Applicable at the end of month";
                    amsg.AccBal = ca.CBal;
                    return amsg;
                }    
                else
                {
                    ca.CBal = ca.CBal + value.Balance;
                    amsg.accountId = value.AccountId;
                    amsg.AccType = "Insufficient Fund";
                    amsg.AccBal = ca.CBal;
                    return amsg;
                }
            }
            return null;
        }

        public transactionmsg transfer(transfers value)
        {
            double sb = 0.0, db = 0.0;
            string data = JsonConvert.SerializeObject(value);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(client.BaseAddress + "/Transaction/transfer/", content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data1 = response.Content.ReadAsStringAsync().Result;
                Ts s1 = JsonConvert.DeserializeObject<Ts>(data1);
                transactionmsg ob = new transactionmsg();
                if (s1.Message == "No Warning")
                {
                    if (value.senderAccountId % 2 == 0)
                    {
                        var sas = savingsAccounts.Find(a => a.savingsAccountId == value.senderAccountId);
                        sas.savingsAccountbalance = sas.savingsAccountbalance - value.amount;
                        if (sas.savingsAccountbalance >= 0)
                            sb = sas.savingsAccountbalance;
                        else
                        {
                            sas.savingsAccountbalance = sas.savingsAccountbalance + value.amount;
                            return null;
                        }
                    }
                    else
                    {
                        var cas = currentAccounts.Find(a => a.CAId == value.senderAccountId);
                        cas.CBal = cas.CBal - value.amount;
                        if (cas.CBal >= 0)
                            sb = cas.CBal;
                        else
                        {
                            cas.CBal = cas.CBal + value.amount;
                            return null;
                        }

                    }
                    if (value.receiverAccountId % 2 == 0)
                    {
                        var sa = savingsAccounts.Find(a => a.savingsAccountId == value.receiverAccountId);
                        sa.savingsAccountbalance = sa.savingsAccountbalance + value.amount;
                        db = sa.savingsAccountbalance;
                    }
                    else
                    {
                        var ca = currentAccounts.Find(a => a.CAId == value.receiverAccountId);
                        ca.CBal = ca.CBal + value.amount;
                        db = ca.CBal;
                    }
                    ob.savingsAccountbalance = sb;
                    ob.rbal = db;
                    ob.transferStatus = "Transfer Successfull";
                    return ob;
                }
                else
                {
                    if (value.senderAccountId % 2 == 0)
                    {
                        var sas = savingsAccounts.Find(a => a.savingsAccountId == value.senderAccountId);
                        sas.savingsAccountbalance = sas.savingsAccountbalance - value.amount;
                        if (sas.savingsAccountbalance >= 0)
                            sb = sas.savingsAccountbalance;
                        else
                        {
                            sas.savingsAccountbalance = sas.savingsAccountbalance + value.amount;
                            return null;
                        }

                    }
                    else
                    {
                        var cas = currentAccounts.Find(a => a.CAId == value.senderAccountId);
                        cas.CBal = cas.CBal - value.amount;
                        if (cas.CBal >= 0)
                            sb = cas.CBal;
                        else
                        {
                            cas.CBal = cas.CBal + value.amount;
                            return null;
                        }

                    }
                    if (value.receiverAccountId % 2 == 0)
                    {
                        var sa = savingsAccounts.Find(a => a.savingsAccountId == value.receiverAccountId);
                        sa.savingsAccountbalance = sa.savingsAccountbalance + value.amount;
                        db = sa.savingsAccountbalance;
                    }
                    else
                    {
                        var ca = currentAccounts.Find(a => a.CAId == value.receiverAccountId);
                        ca.CBal = ca.CBal + value.amount;
                        db = ca.CBal;
                    }
                    ob.savingsAccountbalance = sb;
                    ob.rbal = db;
                    ob.transferStatus = "Transfer Successfull.But Service Charge is applicable in sender account";
                    return ob;
                    //return "Sender Account Balance Rs." + sb + ".00\n" + "Receiver Account Balance Rs." + db + ".00\n but service charge will be deducted at the end of month from your account";

                }

            }
            return null;
        }

        List<savingsAccount> IAccountRep.GetSavings()
        {
            throw new NotImplementedException();
        }

        customerAccount IAccountRep.createAccount(int id)
        {
            throw new NotImplementedException();
        }

        IEnumerable<statement> IAccountRep.getAccountStatement(int AccountId, int from_date, int to_date)
        {
            throw new NotImplementedException();
        }

       
    }
    
}
