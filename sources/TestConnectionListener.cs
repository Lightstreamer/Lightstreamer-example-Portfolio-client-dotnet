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

namespace TwoLevelPush_Example
{

    class TestConnectionListener : ClientListener
    {
        private long bytes = 0;

        private bool sessionStarted = false;

        public TestConnectionListener() { }

        public void OnConnectionEstablished()
        {

        }

        public bool isSessionStarted()
        {
            return sessionStarted;
        }

        public void onListenEnd(LightstreamerClient client)
        {
            // throw new NotImplementedException();
        }

        public void onListenStart(LightstreamerClient client)
        {
            // throw new NotImplementedException();
        }

        public void onServerError(int errorCode, string errorMessage)
        {
            Console.WriteLine("Server error: " + errorMessage + " (" + errorCode + ").");
            sessionStarted = false;
        }

        public void onStatusChange(string status)
        {
            Console.WriteLine("Status changed: " + status + ".");
            if (status.StartsWith("CONNECTED:"))
            {
                sessionStarted = true;
                if (status.EndsWith("POLLING"))
                {
                    Console.WriteLine("Warn: polling session!!!!!!!!!!!!!");
                }
            }
        }

        public void onPropertyChange(string property)
        {
            Console.WriteLine("Property changed: " + property + ".");
            if (property.Equals("sessionId"))
            {
                if (!Program.myClient.connectionDetails.SessionId.Equals(""))
                {
                    Console.WriteLine("New Session ID: " + Program.myClient.connectionDetails.SessionId + ".");
                }
            }
            if (property.Equals("currentConnectTimeout"))
            {
                if (Program.myClient.connectionOptions.CurrentConnectTimeout > 0)
                {
                    Console.WriteLine("New Connect Timeout: " + Program.myClient.connectionOptions.CurrentConnectTimeout + ".");
                }
            }
        }
    }

}