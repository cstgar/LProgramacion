using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection.Metadata;

// El namespace del proyecto Cliente
namespace IntegradoraTarjCliente
{
    class Program
    {
        // Instancia del Gestor, que maneja Sockets, JSON y lógica de conexión.
        // Se asume que este archivo hace referencia a las clases ClienteGestor, Solicitud, y RespuestaServidor.
        private static ClienteGestor Gestor = new ClienteGestor();

        static void Main(string[] args)
        {
            Console.WriteLine("--- CLIENTE DE GESTIÓN DE TARJETAS (SOCKETS) ---");

            // 1. Validar la conexión del servidor al inicio (delegada a ClienteGestor)
            if (!Gestor.CheckServerConnection())
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
                            request = BuildDeleteRequest(); // Asumo que renombraste BuildDeleteQuery a BuildDeleteRequest
                            break;
                        case "5":
                            return; // Salir
                        default:
                            Console.WriteLine("Opción no válida.");
                            continue;
                    }

                    if (!string.IsNullOrEmpty(request))
                    {
                        string response = Gestor.SendRequest(request); // Delegar envío
                        Gestor.ProcessResponse(response);              // Delegar procesamiento de JSON
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
        //                           INTERFAZ (MÉTODOS DE CONSTRUCCIÓN DE PAYLOAD)
        // --------------------------------------------------------------------------------

        static void DisplayMenu()
        {
            Console.WriteLine("--- GESTIÓN DE TARJETAS DE CRÉDITO ---");
            Console.WriteLine("1. Ingresar Nueva Solicitud");
            Console.WriteLine("2. Consultar Cliente por Folio");
            Console.WriteLine("3. Modificar Cliente");
            Console.WriteLine("4. Eliminar Cliente");
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

            // Protocolo de texto plano: INSERTAR|Folio;Nombre;...;Limite
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
            Console.Write("Folio del Cliente a MODIFICAR: ");
            string folio = Console.ReadLine();

            Console.Write("Nuevo Nombre del Cliente (Dejar vacio para no modificar): ");
            string nombre = Console.ReadLine();
            Console.Write("Nueva Dirección: ");
            string direccion = Console.ReadLine();
            Console.Write("Nuevo Telefono Celular: ");
            string telefono = Console.ReadLine();
            Console.Write("Nuevo Correo: ");
            string correo = Console.ReadLine();
            string ocupacionPlaceholder = "";
            decimal limitePlaceholder = 0.00m;
            // Protocolo de texto plano: MODIFICAR|Folio;Nombre;Direccion;...
            string payload = 
                $"{folio};" +
                $"{nombre};" +
                $"{direccion};" +
                $"{telefono};" +
                $"{correo};" +
                $"{ocupacionPlaceholder};" +
                $"{limitePlaceholder.ToString(CultureInfo.InvariantCulture)}";
            return $"MODIFICAR|{payload}";
        }

        static string BuildDeleteRequest()
        {
            Console.WriteLine("--- ELIMINAR SOLICITUD ---");
            Console.Write("Folio del Cliente a ELIMINAR: ");
            string folio = Console.ReadLine();

            return $"ELIMINAR|{folio}";
        }
    }
}