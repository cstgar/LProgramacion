// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
//Console.ReadKey();
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Globalization; // Para decimal.TryParse y ToString es para manejar las diferencias culturales y las convenciones de idioma al momento de mostrar, formatear o manipular datos

namespace IntegradoraTarjCliente
{
    class Program
    {
        // ** CONFIGURACIÓN DE CONEXIÓN **
        private const string SERVER_IP = "25.0.248.40"; // IP del servidor de Sockets
        private const int PORT = 58765;
        private const string EOF = "<EOF>";

        static void Main(string[] args)
        {
            Console.WriteLine("--- CLIENTE DE GESTIÓN DE TARJETAS (SOCKETS) ---");

            // 1. Validar la conexión del servidor al inicio
            if (!CheckServerConnection())
            {
                Console.WriteLine("\nPresione cualquier tecla para salir...");
                Console.ReadKey();
                return; // Salir si el servidor no está disponible
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Conexión al servidor establecida con éxito. \n");
            Console.ResetColor();

            // 2. Bucle principal del menú
            while (true)
            {
                DisplayMenu();
                string option = Console.ReadLine();

                try
                {
                    string request = string.Empty;
                    switch (option)
                    {
                        case "1":
                            request = BuildInsertRequest();
                            break;
                        case "2":
                            request = BuildQueryRequest();
                            break;
                        case "3":
                            request = BuildUpdateRequest();
                            break;
                        case "4":
                            request = BuildDeleteQuery();
                            break;
                        case "5":
                            return; // Salir
                        default:
                            Console.WriteLine("Opción no válida.");
                            continue;
                    }

                    if (!string.IsNullOrEmpty(request))
                    {
                        string response = SendRequest(request);
                        ProcessResponse(response);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\n--- ERROR DE COMUNICACIÓN O DE CLIENTE ---");
                    Console.WriteLine($"Mensaje: {e.Message}");
                }

                Console.WriteLine("\nPresiona Enter para continuar...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        // --------------------------------------------------------------------------------
        //                           VERIFICACIÓN DE CONEXIÓN
        // --------------------------------------------------------------------------------

        static bool CheckServerConnection()
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


        // --------------------------------------------------------------------------------
        //                           INTERFAZ Y CONSTRUCCIÓN DE SOLICITUDES
        // --------------------------------------------------------------------------------

        static void DisplayMenu()
        {
            Console.WriteLine("--- GESTIÓN DE TARJETAS DE CRÉDITO ---");
            Console.WriteLine("1. Ingresar Nueva Solicitud");
            Console.WriteLine("2. Consultar Cliente por Folio");
            Console.WriteLine("3. Modificar Cliente (Pendiente)");
            Console.WriteLine("4. Eliminar Cliente (Pendiente)");
            Console.WriteLine("5. Salir");
            Console.Write("Seleccione una opción: ");
        }

        static string BuildInsertRequest()
        {
            Console.Write("Folio: ");
            string folio = Console.ReadLine();
            Console.Write("Nombre del Cliente: ");
            string nombre = Console.ReadLine();
            Console.Write("Dirección: ");
            string direccion = Console.ReadLine();
            Console.Write("Teléfono Celular: ");
            string telefono = Console.ReadLine();
            Console.Write("Correo: ");
            string correo = Console.ReadLine();
            Console.Write("Ocupación: ");
            string ocupacion = Console.ReadLine();

            string status = "Rechazado";
            decimal limite = 0.00m;

            // Lógica de Decisión (Aprobado/Rechazado)
            while (true)
            {
                Console.Write("¿La solicitud debe ser Aprobada (A) o Rechazada (R)? ");
                string decision = Console.ReadLine().ToUpper();
                if (decision == "A")
                {
                    status = "Aprobado";
                    Console.Write("Ingrese Límite de Crédito (10000.00 a 150000.00): ");
                    // Usar InvariantCulture para asegurar el parseo de punto decimal
                    if (decimal.TryParse(Console.ReadLine(), NumberStyles.Currency, CultureInfo.InvariantCulture, out limite) && limite >= 10000.00m && limite <= 150000.00m)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Límite no válido o fuera de rango. Se asignará el mínimo ($10,000.00).");
                        limite = 10000.00m;
                        break;
                    }
                }
                else if (decision == "R")
                {
                    status = "Rechazado";
                    break;
                }
                else
                {
                    Console.WriteLine("Opción no válida. Use 'A' o 'R'.");
                }
            }

            // Payload: Folio;Nombre;Direccion;Telefono;Correo;Ocupacion;Status;Limite
            string payload = $"{folio};{nombre};{direccion};{telefono};{correo};{ocupacion};{status};{limite.ToString(CultureInfo.InvariantCulture)}";
            return $"INSERTAR|{payload}";
        }

        static string BuildQueryRequest()
        {
            Console.Write("Folio del Cliente a Consultar: ");
            string folio = Console.ReadLine();
            return $"CONSULTAR|{folio}";
        }

        static string BuildUpdateRequest()
        {
            Console.WriteLine("--- MODIFICAR SOLICITUD ---");
            Console.WriteLine("Folio del Cliente a Modificar");
            string folio = Console.ReadLine();
            Console.WriteLine("Nuevo Nombre del Cliente");
            string nombre = Console.ReadLine();
            Console.WriteLine("\n[Nuevos Datos de Contacto y Ocupación]");
            Console.Write("Nueva Dirección: "); 
            string direccion = Console.ReadLine();
            Console.Write("Nuevo Teléfono Celular: ");
            string telefono = Console.ReadLine();
            Console.Write("Nuevo Correo: ");
            string correo = Console.ReadLine();
            Console.Write("Nueva Ocupación: ");
            string ocupacion = Console.ReadLine();
            decimal limite = 0.00m;
            Console.Write("Nuevo Límite de Crédito (Ej: 150000.00): "); 
            // Intenta parsear la entrada. Usamos InvariantCulture para evitar problemas con comas/puntos decimales.
            if (!decimal.TryParse(Console.ReadLine(), NumberStyles.Currency, CultureInfo.InvariantCulture, out limite)) 
            { 
                Console.WriteLine("[Advertencia] Formato de límite inválido, se enviará 0.00."); 
                limite = 0.00m; 
            } 
            // Payload: MODIFICAR|Folio;Direccion;Telefono;Correo;Ocupacion;LimiteCredito
            string payload = $"{folio};{nombre};{direccion};{telefono};{correo};{ocupacion};{limite.ToString(CultureInfo.InvariantCulture)}";
            return $"MODIFICAR|{payload}"; 
        }

        static string BuildDeleteQuery()
        {
            Console.WriteLine("--- Eliminar SOLICITUD ---");
            Console.WriteLine("Folio del Cliente a Eliminar");
            Console.ReadLine();

            string folio = Console.ReadLine();
            return $"ELIMINAR|{folio}";
        }

        // --------------------------------------------------------------------------------
        //                           COMUNICACIÓN SOCKET
        // --------------------------------------------------------------------------------

        static string SendRequest(string request)
        {
            string responseData = string.Empty;

            IPAddress ipAddress = IPAddress.Parse(SERVER_IP);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, PORT);

            using (Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                // Conectar al servidor (nueva conexión por cada solicitud)
                sender.Connect(remoteEP);
                Console.WriteLine($"Conectado y enviando solicitud...");;

                // Enviar la solicitud + el marcador EOF
                byte[] msg = Encoding.ASCII.GetBytes(request + EOF);
                sender.Send(msg);

                // Recibir la respuesta del servidor
                byte[] bytes = new byte[2048];
                int bytesRec = sender.Receive(bytes);
                responseData = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
                return responseData;
            }
        }

        // --------------------------------------------------------------------------------
        //                           PROCESAMIENTO DE RESPUESTA
        // --------------------------------------------------------------------------------

        static void ProcessResponse(string response)
        {
            string[] parts = response.Split(new char[] { '|' }, 2);
            string status = parts[0];
            string message = parts.Length > 1 ? parts[1] : string.Empty;

            if (status == "OK")
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n--- SERVIDOR OK ---");

                if (message.StartsWith("Id=")) // Es una consulta
                {
                    Console.WriteLine("\n--- DETALLES DEL CLIENTE ENCONTRADO ---");
                    string[] fields = message.Split(';');
                    foreach (var field in fields)
                    {
                        if (field.Contains("StatusSolicitud=Aprobado") && field.StartsWith("StatusSolicitud"))
                        {
                            Console.WriteLine("=====================================");
                            Console.WriteLine($"** {field} **");
                            Console.WriteLine("--- DATOS DE TARJETA ---");
                        }
                        else if (field.Contains("StatusSolicitud=Rechazado") && field.StartsWith("StatusSolicitud"))
                        {
                            Console.WriteLine("=====================================");
                            Console.WriteLine($"** {field} **");
                            Console.WriteLine("-------------------------------------");
                        }
                        else
                        {
                            Console.WriteLine(field);
                        }
                    }
                    Console.WriteLine("-------------------------------------");
                }
                else // Es un mensaje de operación
                {
                    Console.WriteLine($"Mensaje: {message}");
                }
            }
            else if (status == "ERROR")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n!!! ERROR DEL SERVIDOR !!!");
                Console.WriteLine($"Mensaje: {message}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n--- RESPUESTA INESPERADA ---");
                Console.WriteLine($"Respuesta Cruda: {response}");
            }
            Console.ResetColor();
        }
    }
}