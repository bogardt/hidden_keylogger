using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace keylogger_client
{
    public class SynchronousClient
    {
        /// <summary>
        /// Connect
        /// Send msg synchronously
        /// and disconnect
        /// </summary>
        /// <param name="msg"></param>
        public static void Send(string msg)
        {
            try
            {
                var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                var ipAddress = ipHostInfo.AddressList[0];
                try
                {
                    var remoteEP = new IPEndPoint(ipAddress, 11000);
                    var sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    sender.Connect(remoteEP);
                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());
                    var data = Encoding.ASCII.GetBytes($"{msg}<EOF>");
                    var bytesSent = sender.Send(data);
                    var bytes = new byte[1024];
                    var bytesRec = sender.Receive(bytes);
                    Console.WriteLine("Echoed test = {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
