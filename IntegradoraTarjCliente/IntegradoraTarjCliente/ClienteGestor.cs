// DENTRO DE ClienteGestor.cs

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IntegradoraTarjCliente
{
    // Esta clase maneja la comunicación de red y la deserialización de JSON
    public class ClienteGestor
    {
        private const string SERVER_IP = "25.0.248.40";
        private const int PORT = 58765;
        private const string EOF = "<EOF>";

        // --- MÉTODOS DE VERIFICACIÓN Y ENVÍO ---

        public bool CheckServerConnection()
        {
            Console.WriteLine($"Intentando conectar con el servidor ({SERVER_IP}:{PORT})...");
            try
            {
                IPAddress ipAddress = IPAddress.Parse(SERVER_IP);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, PORT);

                // Creamos un socket temporal para la prueba
                using (Socket testSender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                {
                    testSender.ReceiveTimeout = 3000; // 3 segundos
                    testSender.SendTimeout = 3000;    // 3 segundos

                    testSender.Connect(remoteEP);
                    testSender.Shutdown(SocketShutdown.Both);
                    testSender.Close();
                    return true;
                }
            }
            catch (SocketException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: No se pudo conectar al servidor.");
                Console.WriteLine($"Asegúrese que el servidor ({SERVER_IP}:{PORT}) está corriendo.");
                Console.WriteLine($"Detalle: {ex.Message}");
                Console.ResetColor();
                return false;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error desconocido durante la verificación: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        // DENTRO DE ClienteGestor.cs
        // (Asumiendo que las constantes SERVER_IP, PORT, y EOF están definidas arriba)

        public string SendRequest(string request)
        {
            string responseData = string.Empty;

            // Convertir las constantes a objetos de red
            IPAddress ipAddress = IPAddress.Parse(SERVER_IP);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, PORT);

            // Utilizamos 'using' para asegurar que el socket se cierre automáticamente
            using (Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    // 1. Conectar al servidor
                    sender.Connect(remoteEP);
                    Console.WriteLine($"Conectado y enviando solicitud...");

                    // 2. Enviar la solicitud + el marcador EOF (End Of File)
                    byte[] msg = Encoding.ASCII.GetBytes(request + EOF);
                    sender.Send(msg);

                    // 3. Recibir la respuesta completa del servidor
                    // Aumentamos el buffer a 2048 para asegurar que quepa el JSON completo.
                    byte[] bytes = new byte[2048];
                    int bytesRec = sender.Receive(bytes);
                    responseData = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    // 4. Cerrar la conexión
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                    return responseData;
                }
                catch (SocketException ex)
                {
                    // Capturar errores específicos de la red si la conexión falla después de la verificación inicial
                    throw new Exception($"Fallo de Socket durante el envío de datos. Verifique la IP/Puerto: {ex.Message}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error desconocido al enviar la solicitud: {ex.Message}");
                }
            }
        }

        // --- MÉTODOS DE PROCESAMIENTO ---

        public void ProcessResponse(string response)
        {
            // ¡IMPLEMENTACIÓN CLAVE! Adaptar a JSON
            try
            {
                RespuestaServidor respuesta = JsonConvert.DeserializeObject<RespuestaServidor>(response);

                if (respuesta == null) throw new Exception("Respuesta nula del servidor.");

                if (respuesta.Exito)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n--- SERVIDOR OK ---");
                    Console.WriteLine($"Mensaje: {respuesta.Mensaje}");

                    // Mostrar datos solo si existen (Operación CONSULTAR)
                    if (respuesta.Solicitudes != null && respuesta.Solicitudes.Count > 0)
                    {
                        Console.WriteLine("\n--- DETALLES DEL CLIENTE ---");
                        foreach (var s in respuesta.Solicitudes)
                        {
                            //Presentacion de Consulta
                            Console.WriteLine($"\n--------------------------------");
                            Console.WriteLine($"FOLIO: {s.Folio}");
                            Console.WriteLine($"NOMBRE: {s.NombreCliente}");
                            Console.WriteLine($"DIRECCION: {s.Direccion}");
                            Console.WriteLine($"CELULAR: {s.TelefonoCelular}");
                            Console.WriteLine($"CORREO: {s.Correo}");
                            Console.WriteLine($"OCUPACION: {s.Ocupacion}");
                            Console.WriteLine($"ESTATUS: {s.StatusSolicitud}");

                            if (s.StatusSolicitud == "Aprobado" && s.LimiteCredito.HasValue)
                            {
                                Console.WriteLine($"\n*** DATOS DE TARJETA ASIGNADOS ***");
                                Console.WriteLine($"LIMITE DE CREDITO: {s.LimiteCredito.Value.ToString("C", CultureInfo.CurrentCulture)}");
                                Console.WriteLine($"NUMERO DE TARJETA: {s.NumTarjeta}");
                                Console.WriteLine($"FECHA DE CORTE: {s.FechaCorte.Value.ToShortDateString()}");
                                Console.WriteLine($"CLABE SPEI: {s.ClabeSPEI}");
                                Console.WriteLine("**********************************************");
                            }
                        }
                    }
                }
                else // Fallo del servidor (Exito = false)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n!!! ERROR DEL SERVIDOR !!!");
                    Console.WriteLine($"Mensaje: {respuesta.Mensaje}");
                }
            }
            catch (JsonException)
            {
                // Si la cadena no es JSON válida
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n--- RESPUESTA INESPERADA (No es formato JSON) ---");
                Console.WriteLine($"Respuesta Cruda: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar la respuesta: {ex.Message}");
            }
            Console.ResetColor();
        }

        
    }
}