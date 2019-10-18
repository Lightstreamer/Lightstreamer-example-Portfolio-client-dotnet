using com.lightstreamer.client;
using System;
using System.Text;

namespace TwoLevelPush_Example
{
    internal class SystemOutSubscriptionListener : SubscriptionListener
    {
        private LightstreamerClient myClient;
        private string v;

        public SystemOutSubscriptionListener(LightstreamerClient myClient, string v)
        {
            this.myClient = myClient;
            this.v = v;
        }

        public void onClearSnapshot(string itemName, int itemPos)
        {
            Console.WriteLine("Server has cleared the current status of the portfolio");
        }

        public void onCommandSecondLevelItemLostUpdates(int lostUpdates, string key)
        {
            Console.WriteLine(lostUpdates + " messages were lost (" + key + ")");
        }

        public void onCommandSecondLevelSubscriptionError(int code, string message, string key)
        {
            Console.WriteLine("Cannot subscribe (2nd-level item " + key + ") because of error " + code + ": " + message);
        }

        public void onEndOfSnapshot(string itemName, int itemPos)
        {
            Console.WriteLine("Initial portfolio received");
        }

        public void onItemLostUpdates(string itemName, int itemPos, int lostUpdates)
        {
            Console.WriteLine(lostUpdates + " messages were lost");
        }

        public void onItemUpdate(ItemUpdate update)
        {
            string command = update.getValue("command");
            if (command.Equals("ADD"))
            {
                Console.WriteLine("first update for this key (" + update.getValue("key") + "), the library is now automatically subscribing the second level item for it");
            }
            else if (command.Equals("UPDATE"))
            {
                StringBuilder updateString = new StringBuilder("Update for ");
                updateString.Append(update.getValue("stock_name")); //2nd level field
                updateString.Append(", last price is ");
                updateString.Append(update.getValue("last_price")); //2nd level field
                updateString.Append(" (");
                updateString.Append(update.getValue("time")); // 2nd level field
                updateString.Append("), we own ");
                updateString.Append(update.getValue("qty")); //1st level field
                updateString.Append(" --> ");
                updateString.Append(getCtv(update));

                Console.WriteLine(updateString);

                //there is the possibility that a second update for the first level is received before the first update for the second level
                //thus we might print a message that contains a few NULLs 
            }
            else if (command.Equals("DELETE"))
            {
                Console.WriteLine("key (" + update.getValue("key") + "), was removed, the library is now automatically unsubscribing the second level item for it");
            }
        }

        public void onListenEnd(Subscription subscription)
        {
            Console.WriteLine("Stop listeneing to subscription events");
        }

        public void onListenStart(Subscription subscription)
        {
            Console.WriteLine("Start listeneing to subscription events");
        }

        public void onRealMaxFrequency(string frequency)
        {
            Console.WriteLine("Frequency is " + frequency);
        }

        public void onSubscription()
        {
            Console.WriteLine("Now subscribed to the portfolio item");
        }

        public void onSubscriptionError(int code, string message)
        {
            Console.WriteLine("Cannot subscribe because of error " + code + ": " + message);
        }

        public void onUnsubscription()
        {
            Console.WriteLine("Now unsubscribed to the portfolio item");
        }

        private string getCtv(ItemUpdate update)
        {
            long newqty;
            double newprice;

            if ( Int64.TryParse(update.getValue("qty"), out newqty) )
            {
                if (Double.TryParse(update.getValue("last_price"), out newprice) )
                {
                    return "" + (newqty * newprice);
                } else
                {
                    return "###";
                }
            } else
            {
                return "###";
            }
            
        }
    }
}