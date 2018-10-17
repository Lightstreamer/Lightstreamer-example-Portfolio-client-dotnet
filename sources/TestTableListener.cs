#region License
/*
* Copyright (c) Lightstreamer Srl
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
#endregion License

using Lightstreamer.DotNetStandard.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TwoLevelPush_Example
{

    public class TestTableListenerForExtended : IHandyTableListener
    {
        private Portfolio myPortfolio;

        public TestTableListenerForExtended(Portfolio myPortfolio) {
            this.myPortfolio = myPortfolio;
        }

        private string NotifyUpdate(IUpdateInfo update)
        {
            return update.Snapshot ? "snapshot" : "update";
        }

        private string NotifyValue(IUpdateInfo update, string fldName)
        {
            string newValue = update.GetNewValue(fldName);
            string notify = " " + fldName + " = " + (newValue != null ? newValue : "null");
            if (update.IsValueChanged(fldName))
            {
                string oldValue = update.GetOldValue(fldName);
                notify += " (was " + (oldValue != null ? oldValue : "null") + ")";
            }
            return notify;
        }

        public void OnUpdate(int itemPos, string itemName, IUpdateInfo update)
        {
            /* Console.WriteLine(NotifyUpdate(update) +
                             " for " + NotifyValue(update, "stock_name")  + ":" +

                             NotifyValue(update, "last_price") +
                             NotifyValue(update, "time"));
                             */

            if (update.IsValueChanged("stock_name"))
            {
                lock (myPortfolio)
                {
                    myPortfolio.addName(itemName, update.GetNewValue("stock_name"));
                }
            }
            if ( update.IsValueChanged("last_price") )
            {
                lock (myPortfolio)
                { 
                    double px;
                    Double.TryParse(update.GetNewValue("last_price"), out px);
                    myPortfolio.updateQuote(itemName, px/100);
                }
            }
        }

        public void OnSnapshotEnd(int itemPos, string itemName)
        {
            Console.WriteLine("end of snapshot for " + itemName);
        }

        public void OnRawUpdatesLost(int itemPos, string itemName, int lostUpdates)
        {
            Console.WriteLine(lostUpdates + " updates lost for " + itemName);
        }

        public void OnUnsubscr(int itemPos, string itemName)
        {
            Console.WriteLine("unsubscr " + itemName);
        }

        public void OnUnsubscrAll()
        {
            Console.WriteLine("unsubscr table");
        }
    }

    public class TestPortfolioListenerForExtended : IHandyTableListener
    {

        private IDictionary<string, SubscribedTableKey> tableRefs = new Dictionary<string, SubscribedTableKey>();
        private LSClient myClient;
        private Portfolio myPortfolio;

        private TaskScheduler myScheduler;

        public TestPortfolioListenerForExtended(LSClient client, string id) {
            myClient = client;

            myPortfolio = new Portfolio(id);
        }

        private string NotifyUpdate(IUpdateInfo update)
        {
            return update.Snapshot ? "snapshot" : "update";
        }

        private string NotifyValue(IUpdateInfo update, string fldName)
        {
            string newValue = update.GetNewValue(fldName);
            string notify = " " + fldName + " = " + (newValue != null ? newValue : "null");
            if (update.IsValueChanged(fldName))
            {
                string oldValue = update.GetOldValue(fldName);
                notify += " (was " + (oldValue != null ? oldValue : "null") + ")";
            }
            return notify;
        }

        public void OnUpdate(int itemPos, string itemName, IUpdateInfo update)
        {
            String cmdU = update.GetNewValue("command");
            String myItem = update.GetNewValue("key");

            Console.WriteLine(NotifyUpdate(update) +
                            " for " + itemName + ":" +
                            " key = " + myItem +
                            " command = " + cmdU +
                            NotifyValue(update, "qty"));

            if (cmdU.Equals("ADD"))
            {
                // *******************************************************************************
                // *********************** Add new Two level subscription.
                // *******************************************************************************

                var t = Task.Run(() => {
                    
                    ExtendedTableInfo tableInfo = new ExtendedTableInfo(
                    new string[] { myItem },
                    "MERGE",
                    new string[] { "stock_name", "last_price", "time" },
                    true
                    );

                    tableInfo.DataAdapter = "QUOTE_ADAPTER";

                    Console.WriteLine("Task thread ID: {0} for Item {1}.", Thread.CurrentThread.ManagedThreadId, myItem);

                    lock (myClient)
                    {
                        SubscribedTableKey tableRef = myClient.SubscribeTable(
                        tableInfo,
                        new TestTableListenerForExtended(myPortfolio),
                        false
                        );
                        tableRefs.Add(myItem, tableRef);
                    }

                    

                    Console.WriteLine("Task thread ID: {0} for Item {1}.", Thread.CurrentThread.ManagedThreadId, myItem);
                });

                lock (myPortfolio)
                {
                    long newqty;
                    Int64.TryParse(update.GetNewValue("qty"), out newqty);
                    myPortfolio.addNewStock(myItem, newqty);
                }

                Console.WriteLine("Add new Two level subscription for " + myItem + ".");
            } else if (cmdU.Equals("DELETE"))
            {
                // *******************************************************************************
                // *********************** Remove existing Two level subscription.
                // *******************************************************************************
                if (tableRefs.ContainsKey(myItem)) {
                    SubscribedTableKey tableRef;
                    tableRefs.TryGetValue(myItem, out tableRef);

                    if (tableRef == null) {
                        Console.WriteLine("Something wrong in subscriptions map.");
                    } else {
                        tableRefs.Remove(myItem);

                        var t = Task.Run(() =>
                        {
                            Console.WriteLine("Task thread ID: {0}", Thread.CurrentThread.ManagedThreadId);
                            lock (myClient)
                            {
                                myClient.UnsubscribeTable(tableRef);
                            }
                        });

                        lock (myPortfolio)
                        {
                            myPortfolio.deleteStock(myItem);
                        }
                    }

                    Console.WriteLine("Remove existing Two level subscription for " + myItem + ".");
                } else
                {
                    Console.WriteLine("Something wrong in subscriptions map.");
                }
            } else
            {
                // Update qty.
                lock (myPortfolio)
                {
                    long newqty;
                    Int64.TryParse(update.GetNewValue("qty"), out newqty);
                    myPortfolio.updateShares(myItem, newqty);
                }
            }
        }

        public void OnSnapshotEnd(int itemPos, string itemName)
        {
            Console.WriteLine("end of snapshot for " + itemName);
        }

        public void OnRawUpdatesLost(int itemPos, string itemName, int lostUpdates)
        {
            Console.WriteLine(lostUpdates + " updates lost for " + itemName);
        }

        public void OnUnsubscr(int itemPos, string itemName)
        {
            Console.WriteLine("unsubscr " + itemName);
        }

        public void OnUnsubscrAll()
        {
            Console.WriteLine("unsubscr table");
        }
    }
}
