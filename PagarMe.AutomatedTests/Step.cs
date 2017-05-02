using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using paymentMethod = PagarMe.Mpos.PaymentMethod;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using PagarMe.Mock.LibC;

namespace PagarMe.AutomatedTests
{
    [Binding]
    public class Step
    {
        public Step()
        {
            var assemblyPath = GetType().Assembly.Location;
            var assemblyDirectory = new FileInfo(assemblyPath).DirectoryName;
            Directory.SetCurrentDirectory(assemblyDirectory);
        }
        

        [Given(@"I use the mock")]
        public void GivenIUseTheMock()
        {
            DllConnect.SetMock();
            Processor = new PaymentProcessor(DllConnect.Stream);
        }

        [Given(@"I use the machine")]
        public void GivenIUseTheMachine()
        {
            DllConnect.SetMachine();
            Processor = new PaymentProcessor(DllConnect.Stream);
        }



        [Given(@"I have a transaction with (Debit|Credit) for value (\d+)")]
        public void GivenATransactionWithValue(paymentMethod paymentMethod, Int32 value)
        {
            PaymentMethod = paymentMethod;
            TransactionValue = value;
        }

        [Given(@"there is a problem on initialization")]
        public void GivenThereIsAProblemOnInitialization()
        {
            DllConnect.SetInitError(Mpos.Mpos.Native.Error.Error);
        }

        [Given(@"there is a problem on updating tables")]
        public void GivenThereIsAProblemOnUpdatingTables()
        {
            DllConnect.SetUpdateTableError(Mpos.Mpos.Native.Error.Error);
        }

        [When(@"the transaction is processed")]
        public void WhenTheTransactionIsProcessed()
        {
            try
            {
                var task = Processor.Pay(PaymentMethod, TransactionValue);
                Task.WaitAny(task);

                Exception = task.Exception;
                ResultList = Processor.FinalResult;
            }
            catch (Exception e)
            {
                Exception = e;
            }
        }

        [Then(@"the exception will( not)? be empty")]
        public void ThenErrorWillBeEmpty(Boolean empty)
        {
            if (empty)
            {
                Assert.IsNull(Exception, Exception?.Message);
            }
            else
            {
                Assert.IsNotNull(Exception);
            }
        }

        [Then(@"the result will (not )?contain")]
        public void ThenTheResultWillBe(Boolean contain, Table resultTable)
        {
            var expectedList = resultTable.Rows.Select(r => r["Text"]).ToList();
            var receivedList = ResultList.Select(r => r.Trim()).ToList();

            for (var r = 0; r < receivedList.Count; r++)
            {
                if (!expectedList.Contains(receivedList[r]))
                {
                    receivedList.Remove(receivedList[r]);
                    r--;
                }
            }

            var received = String.Join(Environment.NewLine, receivedList);
            var expected = String.Join(Environment.NewLine, expectedList);

            if (contain)
            {
                Assert.AreEqual(expected, received,
                    "The messages received are wrong.");
            }
            else
            {
                Assert.AreNotEqual(expected, received,
                    "The messages received are wrong.");
            }
        }






        [StepArgumentTransformation]
        public Boolean EnglishBool(String s)
        {
            return s.ToLower() != " not" && s.ToLower() != "not ";
        }

        private paymentMethod PaymentMethod
        {
            get { return get<paymentMethod>("PaymentMethod"); }
            set { set("PaymentMethod", value); }
        }

        private Int32 TransactionValue
        {
            get { return get<Int32>("TransactionValue"); }
            set { set("TransactionValue", value); }
        }

        private Exception Exception
        {
            get { return get<Exception>("Exception"); }
            set { set("Exception", value); }
        }

        private IList<String> ResultList
        {
            get { return get<IList<String>>("ResultList"); }
            set { set("ResultList", value); }
        }

        private PaymentProcessor Processor
        {
            get { return get<PaymentProcessor>("Processor"); }
            set { set("Processor", value); }
        }

        private T get<T>(String key)
        {
            return ScenarioContext.Current.ContainsKey(key)
                ? ScenarioContext.Current.Get<T>(key)
                : default(T);
        }

        private void set<T>(String key, T value)
        {
            if (ScenarioContext.Current.ContainsKey(key))
                ScenarioContext.Current[key] = value;
            else
                ScenarioContext.Current.Add(key, value);
            
        }


    }
}
