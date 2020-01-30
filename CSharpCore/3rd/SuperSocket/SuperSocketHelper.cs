using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace SuperSocket
{
    public class SuperSocketHelper : EngineNS.BrickDescriptor
    {
        SocketServerBase server;
        public override async System.Threading.Tasks.Task DoTest()
        {
            server = new SocketServerBase(new SocketServerBaseDesc());
            server.NewClientAccepted += Server_NewClientAccepted;
            server.CloseClientConnect += Server_CloseClientConnect;
            server.Start();

            TestClient();

            await base.DoTest();
        }
        private void Server_NewClientAccepted(Socket client, ISocketSession session)
        {
            Console.WriteLine("----- new client ------------");
            AsyncSocketSession ass = session as AsyncSocketSession;
            ass.ExtObject = "Fuck";

            ass.SetReceiveHandler(arg =>
            {
                Console.WriteLine("----- new receive ------------");
                string received = System.Text.Encoding.UTF8.GetString(arg.Buffer, arg.Offset, arg.BytesTransferred);
                Console.WriteLine(received);

                ass.Send(received);
            });
        }
        private void Server_CloseClientConnect(ISocketSession session)
        {
            AsyncSocketSession ass = session as AsyncSocketSession;
            Console.WriteLine(ass.ExtObject);
        }
        public void TestClient()
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                client.Connect("127.0.0.1", 2020);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            for (int i = 0; i < 10; i++)
            {
                //try
                //{
                //    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //    client.Connect("127.0.0.1", 2020);
                //}
                //catch (Exception ex)
                //{
                //    throw ex;
                //}

                try
                {
                    client.Send(System.Text.Encoding.UTF8.GetBytes("hello world!"));
                }
                catch (Exception)
                {
                    Console.WriteLine("send error.");
                }
                Console.WriteLine("sent message.");
                var buffer = new byte[128];
                try
                {
                    client.Receive(buffer);
                }
                catch (Exception)
                {
                    Console.WriteLine("receive error.");
                }
                Console.WriteLine("received message.");
                Console.WriteLine(System.Text.Encoding.UTF8.GetString(buffer, 0, 12));

                //var key = Console.ReadKey();

                //if (key.KeyChar.Equals('q'))
                //    break;

                //Console.WriteLine("any key to continue, press q to exit.");
                //try
                //{
                //    Console.WriteLine("---Close Client.---");
                //    client.Shutdown(SocketShutdown.Both);
                //    client.Dispose();
                //}
                //catch (Exception)
                //{
                //    Console.WriteLine("Shundown Error");
                //}

            }

            client.Shutdown(SocketShutdown.Both);
            client.Close();

            server.Stop();
        }
    }
}
