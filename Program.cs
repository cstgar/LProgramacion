// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace clientecsharp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
                {
                    //Crear socket
                    IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                    IPAddress ipAddress = IPAddress.Parse("25.0.248.40");
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11001);
                    Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    sender.Connect(remoteEP); //Conectarse a Asociar IP y puerto para enviar informacion
                    byte[] msg = Encoding.ASCII.GetBytes("Mensjae de texto del cliente <EOF>"); //Codificar cadena de texto a enviar
                    int bytesSent = sender.Send(msg); //Enviar cadena a traves del socket
                    Console.WriteLine("Mensaje enviado\n");
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();  //Cerrar el socket
                    Console.ReadLine();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
        }
    }
}