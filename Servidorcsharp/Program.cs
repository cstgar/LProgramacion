// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketServidor
{
    class Program
    {
        public static string data = null;
        public static void Main(string[] args)
        {
            byte[] bytes = new byte[1024];
            //Crear socket
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = IPAddress.Parse("25.0.248.40");
            IPEndPoint  localEndPoint = new IPEndPoint(ipAddress, 11001);
            Socket listener = new Socket (ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(localEndPoint); //Asociar IP y Puerto
                listener.Listen(10); //Poner socket en escuch apara conexiones

                //Escuchar conexiones y recibir datos cuando un cliente se conecte
                while(true)
                {
                    Console.WriteLine("Esperando por conexion");
                    Socket handler = listener.Accept();
                    data = null;

                    while (true)
                    {
                        int bytesRec = handler.Receive(bytes);  //Mientras haya datos en buffer recibirlos
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }
                    Console.WriteLine(data);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.ToString());
            }
            Console.Read();
        }
    }
}
