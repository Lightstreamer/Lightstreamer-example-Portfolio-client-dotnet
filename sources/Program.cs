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


using com.lightstreamer.client;
using System;
using System.Threading;


namespace TwoLevelPush_Example
{
    class Program
    {

        public static LightstreamerClient myClient;

        static void Main(string[] args)
        {

            Console.WriteLine("Start TwoLevel Push example");

            if (args.Length < 2) {
                Console.WriteLine("Arguments missing. Exit.");

                Thread.Sleep(7000);

                return;
            }
            string pushServerHost = args[0];
            int pushServerPort = Int32.Parse(args[1]);

            Thread.Sleep(2000);
            if ( pushServerPort > 0 )
            {
                Console.WriteLine("Opening Lightstreamer connection (" + pushServerHost + ":" + pushServerPort + ")");
                myClient = new LightstreamerClient("http://" + pushServerHost + ":" + pushServerPort, "DEMO");
            } else
            {
                Console.WriteLine("Opening Lightstreamer connection (" + pushServerHost + ")");
                myClient = new LightstreamerClient("https://" + pushServerHost, "DEMO");
            }
            
            TestConnectionListener myConnectionListener = new TestConnectionListener();
            myClient.addListener(myConnectionListener);

            try
            {
                myClient.connect();
            }
            catch (Exception eu)
            {
                Console.WriteLine("Error: " + eu.Message + ". Exit.");

                Thread.Sleep(7000);

                return;
            }

            while (!myConnectionListener.isSessionStarted())
            {
                Thread.Sleep(70);
            }

            Console.WriteLine("Go for the subscribe.");

            Subscription sub = new Subscription("COMMAND", "portfolio1", new String[] { "key", "command", "qty" });

            sub.DataAdapter = "PORTFOLIO_ADAPTER";
            sub.RequestedSnapshot = "yes";
            sub.CommandSecondLevelDataAdapter = "QUOTE_ADAPTER";
            sub.CommandSecondLevelFields = new String[] { "stock_name", "last_price", "time" }; //the key values from the 1st level are used as item names for the second level

            sub.addListener(new SystemOutSubscriptionListener(myClient, "portfolio1"));

            myClient.subscribe(sub);


            Thread.Sleep(5000);

            while (true)
            {
                Thread.Sleep(20);
            }
        }
    }
}
