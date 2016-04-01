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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoLevelPush_Example
{

    public class ItemDetails {

        private string StockName;
        private string id;
        private long shares;
        private double last;
        private double ctv;

        public ItemDetails(string item, long qty)
        {
            this.id = item;
            this.shares = qty;
        }

        public void setName(string name)
        {
            this.StockName = name;
        }

        public void updateQuote(double price)
        {
            this.last = price;
            this.ctv = this.last * this.shares;

            Console.WriteLine(" >>> " + this.StockName + " <<< " + this.last + " x " + this.shares + " === " + this.ctv);
        }

        public void updateShares(long qty)
        {
            this.shares = qty;
            this.ctv = this.last * this.shares;

            Console.WriteLine(" >>> " + this.StockName + " <<< " + this.last + " x " + this.shares + " === " + this.ctv);
        }
    }

    public class Portfolio
    {
        private string portfolioId;
        private IDictionary<string, ItemDetails> portfolio = new Dictionary<string, ItemDetails>();

        public Portfolio(string id)
        {
            this.portfolioId = id;
        }

        public void addNewStock(string item, long qty)
        {
            portfolio.Add(item, new ItemDetails(item, qty));
        }

        public void deleteStock(string item)
        {
            portfolio.Remove(item);
        }

        public void addName(string item, string name)
        {
            ItemDetails temp;
            portfolio.TryGetValue(item, out temp);
            if (temp != null)
            {
                temp.setName(name);
            }
        }

        public void updateQuote(string item, double price)
        {
            ItemDetails temp;
            portfolio.TryGetValue(item, out temp);
            if (temp != null)
            {
                temp.updateQuote(price);
            }
        }

        public void updateShares(string item, long qty)
        {
            ItemDetails temp;
            portfolio.TryGetValue(item, out temp);
            if (temp != null)
            {
                temp.updateShares(qty);
            }
        }
    }
}
