// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");
// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization; // Necesario para decimal.Parse

namespace SocketServidor
{
    class Program
    {
        // ** CONFIGURACIÓN DE CONEXIÓN **
        // Asegúrate que esta cadena de conexión es accesible por el Servidor de Sockets y que el usuario y password son correctos.
        private const string ConnectionString = "Data Source=25.0.248.40; Database=lp_abc_multi; User Id=access_cinthia; Password=Salome123; TrustServerCertificate=True;";
        private const string SERVER_IP = "25.0.248.40"; // IP donde escucha el servidor
        private const int PORT = 58765;
        private const string EOF = "<EOF>";

        public static void Main(string[] args)
        {
            IPAddress ipAddress = IPAddress.Parse(SERVER_IP);
            //IPAddress ipAddress = IPAddress.Any;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint); //Aqui enlazaria la parte de 0.0.0.0:58765 si se usa Any o IP especifica 
                listener.Listen(10);
                Console.WriteLine($"Servidor iniciado en {ipAddress}:{PORT}. Esperando conexiones...");

                while (true)
                {
                    Socket handler = listener.Accept();

                    IPEndPoint remoteIpEndPoint = handler.RemoteEndPoint as IPEndPoint;

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n--> Conexión ACEPTADA desde: {remoteIpEndPoint.Address} Puerto: {remoteIpEndPoint.Port}");
                    Console.ResetColor();

                    string data = null;
                    byte[] bytes = new byte[2048]; // Aumentado para manejo de datos

                    // 1. Recibir la solicitud completa del cliente
                    while (true)
                    {
                        int bytesRec = handler.Receive(bytes);
                        // Asegurar que solo se decodifican los bytes realmente recibidos
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf(EOF) > -1) break;

                        // Si no se recibió el EOF, pero el buffer se llenó, salir para evitar bucle infinito
                        if (bytesRec < bytes.Length) break;
                    }

                    // Limpiar y procesar
                    string request = data.Replace(EOF, "").Trim();
                    Console.WriteLine($"\nSolicitud recibida: {request}");

                    // 2. Procesar la solicitud y obtener la respuesta
                    string response = ProcessRequest(request);

                    // 3. Enviar la respuesta al cliente
                    byte[] msg = Encoding.ASCII.GetBytes(response);
                    handler.Send(msg);

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n--- ERROR CRÍTICO DEL SERVIDOR ---");
                Console.WriteLine(ex.ToString());
            }
            // Mantenemos la consola abierta solo en caso de un error crítico
            Console.Read();
        }

        private static string ProcessRequest(string request)
        {
            // Protocolo: OPERACION|PAYLOAD
            string[] parts = request.Split(new char[] { '|' }, 2);
            if (parts.Length < 1) return "ERROR|Solicitud invalida o incompleta";

            string operation = parts[0].ToUpper();
            string payload = parts.Length > 1 ? parts[1] : string.Empty;
            string result = string.Empty;

            try
            {
                switch (operation)
                {
                    case "INSERTAR":
                        result = HandleInsert(payload);
                        break;
                    case "CONSULTAR":
                        result = HandleQuery(payload);
                        break;
                    case "MODIFICAR":
                        result = HandleUpdate(payload);
                        break;
                    case "ELIMINAR":
                        result = HandleDelete(payload);
                        break;
                    default:
                        result = "ERROR|Operacion no reconocida";
                        break;
                }
            }
            catch (Exception ex)
            {
                result = $"ERROR|Error de DB o lógica: {ex.Message}";
            }

            return result;
        }

        // --------------------------------------------------------------------------------
        //                           LÓGICA DE NEGOCIO Y DB
        // --------------------------------------------------------------------------------

        private static string HandleInsert(string payload)
        {
            // Payload esperado: Folio;Nombre;Direccion;Telefono;Correo;Ocupacion;Status;Limite
            string[] data = payload.Split(';');

            if (data.Length < 8) return "ERROR|Datos incompletos para insertar. Se esperan 8 campos.";

            // 1. Descomponer datos del cliente
            string folio = data[0];
            string nombre = data[1];
            string direccion = data[2];
            string telefono = data[3];
            string correo = data[4];
            string ocupacion = data[5];
            string status = data[6];
            // Usamos CultureInfo.InvariantCulture para parsear el decimal con punto sin problemas de configuración regional
            decimal limiteCredito = decimal.Parse(data[7], CultureInfo.InvariantCulture);

            // 2. Aplicar Lógica de Negocio (Generación de datos sensibles)
            string numTarjeta = null;
            DateTime? fechaCorte = null;
            string clabeSpei = null;

            if (status == "Aprobado")
            {
                // Revalidar el límite por si el cliente falló o envió un valor fuera del rango
                if (limiteCredito < 10000.00m || limiteCredito > 150000.00m)
                {
                    limiteCredito = 10000.00m; // Asignar el mínimo
                }

                numTarjeta = GenerateCardNumber();
                fechaCorte = DateTime.Today.AddDays(25);
                clabeSpei = GenerateClabe();
            }
            else
            {
                limiteCredito = 0.00m;
            }

            // 3. Ejecutar Insert en DB
            using (SqlConnection conexion = new SqlConnection(ConnectionString))
            {
                conexion.Open();
                string sql = @"
                    INSERT INTO Solicitudes (Folio, NombreCliente, Direccion, TelefonoCelular, Correo, Ocupacion, StatusSolicitud, NumTarjeta, LimiteCredito, FechaCorte, ClabeSPEI)
                    VALUES (@Folio, @Nombre, @Dir, @Tel, @Correo, @Ocupacion, @Status, @NumTarjeta, @Limite, @FechaCorte, @Clabe);";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.Add("@Folio", SqlDbType.VarChar, 50).Value = folio;
                    cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = nombre;
                    cmd.Parameters.Add("@Dir", SqlDbType.VarChar, 255).Value = direccion;
                    cmd.Parameters.Add("@Tel", SqlDbType.VarChar, 20).Value = telefono;
                    cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value = correo;
                    cmd.Parameters.Add("@Ocupacion", SqlDbType.VarChar, 50).Value = ocupacion;
                    cmd.Parameters.Add("@Status", SqlDbType.VarChar, 20).Value = status;

                    // Manejo de valores NULL en SQL Server
                    cmd.Parameters.Add("@NumTarjeta", SqlDbType.VarChar, 20).Value = (object)numTarjeta ?? DBNull.Value;
                    cmd.Parameters.Add("@Limite", SqlDbType.Decimal).Value = limiteCredito;
                    cmd.Parameters.Add("@FechaCorte", SqlDbType.Date).Value = (object)fechaCorte ?? DBNull.Value;
                    cmd.Parameters.Add("@Clabe", SqlDbType.VarChar, 50).Value = (object)clabeSpei ?? DBNull.Value;

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0 ? $"OK|Solicitud {status} insertada con exito." : "ERROR|No se pudo insertar el registro (Folio duplicado?).";
                }
            }
        }

        private static string HandleQuery(string folio)
        {
            string result = string.Empty;

            using (SqlConnection conexion = new SqlConnection(ConnectionString))
            {
                conexion.Open();
                string sql = "SELECT * FROM Solicitudes WHERE Folio = @Folio";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.Add("@Folio", SqlDbType.VarChar, 50).Value = folio;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Formato de respuesta: OK|Campo1=Valor1;Campo2=Valor2;...
                            result = "OK|";
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string fieldName = reader.GetName(i);
                                // Obtener valor y convertir DBNull.Value a cadena vacía
                                object value = reader.IsDBNull(i) ? string.Empty : reader.GetValue(i);
                                result += $"{fieldName}={value.ToString()};";
                            }
                            result = result.TrimEnd(';');
                        }
                        else
                        {
                            result = "ERROR|Folio no encontrado.";
                        }
                    }
                }
            }
            return result;
        }
        private static string HandleUpdate(string payload)
        {
            // Payload esperado: Folio;Direccion;Telefono;Correo;Ocupacion;LimiteCredito
            string[] data = payload.Split(';');

            if (data.Length < 7) return "ERROR|Datos incompletos para modificar. Se esperan 7 campos.";

            try
            {
                // 1. Descomponer y validar datos
                string folio = data[0];
                string nombre = data[1];
                string direccion = data[2];
                string telefono = data[3];
                string correo = data[4];
                string ocupacion = data[5];
                // Usamos InvariantCulture para parsear el decimal enviado desde el Cliente
                decimal limiteCredito = decimal.Parse(data[6], CultureInfo.InvariantCulture);

                // 2. Ejecutar Update en DB
                using (SqlConnection conexion = new SqlConnection(ConnectionString))
                {
                    conexion.Open();

                    // La cláusula CASE WHEN asegura que el LimiteCredito SOLO se actualice
                    // si el StatusSolicitud es 'Aprobado'.
                    string sql = @"
                UPDATE Solicitudes SET
                    NombreCliente = @Nombre,
                    Direccion = @Dir, 
                    TelefonoCelular = @Tel, 
                    Correo = @Correo, 
                    Ocupacion = @Ocupacion, 
                    LimiteCredito = CASE 
                                        WHEN StatusSolicitud = 'Aprobado' THEN @Limite
                                        ELSE LimiteCredito 
                                    END
                WHERE Folio = @Folio";

                    using (SqlCommand cmd = new SqlCommand(sql, conexion))
                    {
                        // Parámetros para evitar inyección SQL
                        cmd.Parameters.Add("@Folio", SqlDbType.VarChar, 50).Value = folio;
                        cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = nombre;
                        cmd.Parameters.Add("@Dir", SqlDbType.VarChar, 255).Value = direccion;
                        cmd.Parameters.Add("@Tel", SqlDbType.VarChar, 20).Value = telefono;
                        cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value = correo;
                        cmd.Parameters.Add("@Ocupacion", SqlDbType.VarChar, 50).Value = ocupacion;
                        cmd.Parameters.Add("@Limite", SqlDbType.Decimal).Value = limiteCredito;

                        int rowsAffected = cmd.ExecuteNonQuery();

                        return rowsAffected > 0
                            ? $"OK|Registro con Folio {folio} modificado con exito. (Campos actualizados: {rowsAffected})"
                            : $"ERROR|No se encontró el Folio {folio} para modificar.";
                    }
                }
            }
            catch (FormatException ex)
            {
                return $"ERROR|Error de formato en los datos: {ex.Message}. Asegúrese de que el límite de crédito sea numérico.";
            }
            catch (Exception ex)
            {
                return $"ERROR|Fallo en el servidor al modificar el registro: {ex.Message}";
            }
        }

        private static string HandleDelete(string payload)
        {
            string folio = payload.Trim();

            if (string.IsNullOrWhiteSpace(folio))
            {
                return "ERROR|Debe proporcionar un Folio para eliminar";
            }

            try
            {
                using (SqlConnection conexion = new SqlConnection(ConnectionString))
                {
                    conexion.Open();
                    //Sentencia SQL: DELETE FROM Solicitudes WHERE Folio = @Folio
                    string sql = "DELETE FROM Solicitudes WHERE Folio = @Folio";

                    using (SqlCommand cmd = new SqlCommand(sql, conexion))
                    {
                        // Parámetros para evitar inyección SQL
                        cmd.Parameters.Add("@Folio", SqlDbType.VarChar, 50).Value = folio;

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Si rowsAffected es mayor que 0, se eliminó un registro.
                            return $"OK|Registro con Folio {folio} eliminado con éxito.";
                        }
                        else
                        {
                            // Si rowsAffected es 0, no se encontró el folio.
                            return $"ERROR|No se encontró ningún registro con Folio {folio} para eliminar.";
                        }
                    }
                }



            }
            catch (Exception ex)
            {
                return $"ERROR|Fallo en el servidor al eliminar el registro: {ex.Message}";
            }

        }

        // --- Funciones Auxiliares para Datos Sensibles ---
        private static string GenerateCardNumber()
        {
            Random r = new Random();
            // Genera un número de tarjeta de 16 dígitos simple (simulación)
            return $"4{r.Next(1000, 9999)}{r.Next(1000, 9999)}{r.Next(1000, 9999)}{r.Next(1000, 9999)}";
        }

        private static string GenerateClabe()
        {
            Random r = new Random();
            // Genera una CLABE de 18 dígitos (simulación)
            return $"{r.Next(100, 999)}{r.Next(100, 999)}{r.Next(100, 999)}{r.Next(1000, 9999)}{r.Next(100, 999)}";
        }
    }
}