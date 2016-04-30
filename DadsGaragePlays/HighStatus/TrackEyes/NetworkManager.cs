using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;

namespace TrackEyes
{
    class StateObject
    {
        //client
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    class NetworkManager
    {
        //data buffer
        private static byte[] buffer = new byte[1024];
        private static ManualResetEvent connectDone = new ManualResetEvent(false);

        public static bool change = false;
        public static string path = null;

        public void init()
        {
            try
            {
                Console.WriteLine("Init");
                //set up socket
                Socket client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
                //IPHostEntry hostInfo = Dns.Resolve("localhost:8000");
                //IPAddress address = hostInfo.AddressList[0];
                //IPAddress ipAddress = Dns.GetHostEntry("localhost:8000").AddressList[0];
                IPAddress ipAddress = new IPAddress(new byte[] { 128, 61, 105, 215 });
                IPEndPoint ep = new IPEndPoint(ipAddress, 8085);
                client.BeginConnect(ep, new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();
                //receiveForever(client);
                byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");
                StateObject state = new StateObject();
                state.workSocket = client;
                client.BeginReceive(buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                client.Send(msg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            Console.WriteLine("Connection made to server. Starting to listen");
            connectDone.Set();
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            Console.WriteLine("Callback");
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);
                Console.WriteLine("Buffer");
                Console.WriteLine(bytesRead);
                Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                //this needs work
                /*if (bytesRead > 0)
                {
                    Console.WriteLine("Grabbing more");
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    byte[] msg = Encoding.ASCII.GetBytes("why");
                    Console.WriteLine("received");
                    Console.WriteLine(state.sb.ToString());
                    client.Send(msg);
                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }*/
                // All the data has arrived; put it in response.
                //if (state.sb.Length > 1)
                //  {
                Console.WriteLine("Finished receiving?");
                //string response = state.sb.ToString();
                Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                string resp = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                byte[] msg = Encoding.ASCII.GetBytes(resp);

                path = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                change = true;
                /*
                String path = "pack://application:,,,/Images/" + Encoding.ASCII.GetString(buffer, 0, bytesRead);
                MainWindow.myMask.Source = new BitmapImage(new Uri(@path));
                */

                client.Send(msg);
                client.BeginReceive(buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
        public void receiveForever(Socket client)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("This is exciting");
                    byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");
                    client.Send(msg);
                    //listen for incoming messages
                    int bytesRec = client.Receive(buffer);
                    Console.WriteLine("Echoed = {0}", Encoding.ASCII.GetString(buffer, 0, bytesRec));

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("Problem communicating with server");
                }
            }

        }

        public bool checkChange()
        {
            return change;
        }
        public void setChange(bool ch)
        {
            change = ch;
        }

        public string getPath()
        {
            return path;
        }
    }
}
