using Newtonsoft.Json;
using System;
using System.Collections.Generic; // Para usar List<T>
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketServidor
{
    class Program
    {
        // ConnectionString, GenerateCardNumber, etc., fueron movidos.
        private const string SERVER_IP = "25.0.248.40";
        private const int PORT = 58765;
        private const string EOF = "<EOF>";

        // Instancia del Gestor para usar en ProcessRequest
        private static readonly GestorTarjetas Gestor = new GestorTarjetas();

        public static void Main(string[] args)
        {
            IPAddress ipAddress = IPAddress.Parse(SERVER_IP);
            // IPAddress ipAddress = IPAddress.Any;

            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // ** 2. INICIO Y ENLACE DEL SOCKET **
                listener.Bind(localEndPoint);
                listener.Listen(10);
                Console.WriteLine($"Servidor iniciado en {ipAddress}:{PORT}. Esperando conexiones...");

                // ** 3. BUCLE PRINCIPAL DE ESCUCHA Y DELEGACIÓN **
                while (true)
                {
                    Socket handler = listener.Accept(); // Aceptar conexión

                    // Obtener IP del cliente para log (opcional)
                    IPEndPoint remoteIpEndPoint = handler.RemoteEndPoint as IPEndPoint;
                    Console.WriteLine($"\n--> Conexión ACEPTADA desde: {remoteIpEndPoint.Address}");

                    // DELEGACIÓN A TAREAS (HILOS): 
                    // Para cumplir con el requisito de Hilos, el manejo debe ser asíncrono.
                    // Task.Run(() => HandleClientConnection(handler));

                    // Por ahora, para mantener el flujo síncrono simple:
                    HandleClientConnection(handler);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n--- ERROR CRÍTICO DEL SERVIDOR ---");
                Console.WriteLine(ex.ToString());
            }
            Console.Read();


        }

        // Función auxiliar que maneja el ciclo de vida de la conexión (mover el código del bucle aquí)
        private static void HandleClientConnection(Socket handler)
        {
            // 1. Declarar e inicializar variables de recepción
            string data = null; // Inicializar a null o string.Empty
            byte[] bytes = new byte[2048]; // Buffer para la recepción

            try
            {
                // === Lógica de recepción de datos (copiada de tu Main original) ===

                // 1. Recibir la solicitud completa del cliente
                while (true)
                {
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    // Revisa si se recibió el marcador de final de archivo
                    if (data.IndexOf(EOF) > -1) break;

                    // Si no se recibió el EOF, pero el buffer se llenó, salir para evitar bucle infinito
                    if (bytesRec < bytes.Length) break;
                }

                // 2. Procesar y delegar
                string request = data.Replace(EOF, "").Trim();

                // El log del Servidor ahora muestra qué hilo está trabajando (opcional)
                Console.WriteLine($"\n[Hilo: {Thread.CurrentThread.ManagedThreadId}] Solicitud recibida: {request}");

                // Procesamiento principal: DELEGACIÓN
                string response = ProcessRequest(request);

                // 3. Enviar la respuesta y cerrar el socket
                byte[] msg = Encoding.ASCII.GetBytes(response);
                handler.Send(msg);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception ex)
            {
                // Manejo de errores de conexión o procesamiento dentro de este hilo
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error en el manejo del cliente: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                // Asegurar que el socket se cierre incluso si hay una excepción
                if (handler != null && handler.Connected)
                {
                    handler.Close();
                }
            }
        }
        private static string ProcessRequest(string request)
        {
            string[] parts = request.Split(new char[] { '|' }, 2);
            if (parts.Length < 1) return SerializarRespuestaError("Solicitud invalida o incompleta");

            string operation = parts[0].ToUpper();
            string payload = parts.Length > 1 ? parts[1] : string.Empty;
            RespuestaServidor respuesta;

            try
            {
                switch (operation)
                {
                    case "LOGIN":
                        //parsear el payload
                        string[] loginParts = payload.Split(';');
                        if (loginParts.Length == 2)
                        {
                            //Llamar a capa de negocio
                            respuesta = Gestor.ValidarCredenciales(loginParts[0], loginParts[1]);
                        }
                        else
                        {
                            respuesta = new RespuestaServidor { Exito = false, Mensaje = "Formato de login incorrecto" };
                        }
                        break;
                    case "INSERTAR":
                        Solicitud nuevaSolicitud = ParsePayloadToSolicitud(payload);
                        respuesta = Gestor.ProcesarNuevaSolicitud(nuevaSolicitud);
                        break;
                    case "CONSULTAR":
                        // Para la consulta, el payload es solo el folio
                        respuesta = Gestor.ConsultarSolicitud(payload.Trim());
                        break;
                    case "MODIFICAR":
                        Solicitud solicitudModificar = ParsePayloadToModificacion(payload);
                        respuesta = Gestor.ProcesarModificacion(solicitudModificar);
                        break;
                    case "ELIMINAR":
                        // El payload es solo el folio
                        respuesta = Gestor.ProcesarEliminacion(payload.Trim());
                        break;
                    default:
                        respuesta = new RespuestaServidor { Exito = false, Mensaje = "Operacion no reconocida." };
                        break;
                }
            }
            catch (Exception ex)
            {
                respuesta = new RespuestaServidor { Exito = false, Mensaje = $"Error al procesar: {ex.Message}" };
            }

            // Devolver la respuesta serializada en JSON
            return Newtonsoft.Json.JsonConvert.SerializeObject(respuesta);
        }

        private static Solicitud ParsePayloadToSolicitud(string payload)
        {
            // El payload esperado es: Folio;Nombre;Direccion;Telefono;Correo;Ocupacion;Status;Limite
            string[] data = payload.Split(';');

            // NOTA: El cliente envía 8 campos de datos, por lo que esperamos length >= 8
            if (data.Length < 8)
            {
                // Lanzar excepción para ser capturada por ProcessRequest
                throw new Exception("Payload de inserción incompleto. Se esperaban 8 campos (Folio;Nombre;...;Limite).");
            }

            // Convertir el límite (que viene como string) a decimal?
            // Usamos CultureInfo.InvariantCulture, que requiere el 'using System.Globalization;'
            if (!decimal.TryParse(data[7], CultureInfo.InvariantCulture, out decimal limite))
            {
                throw new Exception("El límite de crédito no tiene un formato numérico válido.");
            }

            // Devolver el nuevo objeto Solicitud con los datos mapeados
            return new Solicitud
            {
                Folio = data[0],
                NombreCliente = data[1],
                Direccion = data[2],
                TelefonoCelular = data[3],
                Correo = data[4],
                Ocupacion = data[5],
                StatusSolicitud = data[6],

                // Asignar el valor decimal. Si es 0.00m (falla de TryParse), se asigna null.
                LimiteCredito = limite > 0 ? limite : (decimal?)null
            };
        }

        private static Solicitud ParsePayloadToModificacion(string payload)
        {
            // El payload que recibes es: Folio;Nombre;Direccion;Telefono;Correo;Ocupacion;LimiteCredito
            string[] data = payload.Split(';');

            // 1. Validación de longitud: Esperamos 7 campos específicos para la modificación.
            if (data.Length < 7)
            {
                throw new Exception("Payload de modificación incompleto. Se esperaban 7 campos.");
            }

            // 2. Parseo seguro del Límite de Crédito
            // El límite es el último campo (índice 6)
            decimal limite = 0.00m;
            if (!decimal.TryParse(data[6], CultureInfo.InvariantCulture, out limite))
            {
                // Si el parseo falla, el límite será 0.00m. El Gestor/DB lo manejará como un placeholder.
            }

            // 3. Mapeo a Objeto Solicitud
            return new Solicitud
            {
                Folio = data[0],
                NombreCliente = data[1],
                Direccion = data[2],
                TelefonoCelular = data[3],
                Correo = data[4],
                Ocupacion = data[5],

                // El status no se modifica en esta operación, ni se envía desde el cliente.

                // LimiteCredito puede ser null si es 0.00 (placeholder) o si el cliente no lo especificó
                LimiteCredito = limite > 0 ? limite : (decimal?)null
            };
        }

        private static string SerializarRespuestaError(string mensaje)
        {
            // Crear un objeto RespuestaServidor con el flag de fallo
            var errorResponse = new RespuestaServidor
            {
                Exito = false,
                Mensaje = mensaje,
                // Las propiedades Solicitudes y SolicitudUnica se dejan en null
            };

            // Serializar el objeto a JSON
            return Newtonsoft.Json.JsonConvert.SerializeObject(errorResponse);
        }

    }
}