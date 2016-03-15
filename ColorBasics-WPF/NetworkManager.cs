using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    class NetworkManager
    {
        //data buffer
        byte[] bytes = new byte[1024];
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
                IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
                IPEndPoint ep = new IPEndPoint(ipAddress, 8000);
                client.Connect(ep);
                listenForever(client);
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void listenForever(Socket client)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("This is exciting");
                    byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");
                    client.Send(msg);
                    //listen for incoming messages
                    int bytesRec = client.Receive(bytes);
                    Console.WriteLine("Echoed = {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine("Problem communicating with server");
                }
            }

        }
    }
}
