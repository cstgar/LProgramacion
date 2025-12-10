//Codigo asincrono para usar con API y protocolo http
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace IntegradoraTarjCliente
{
    // DTO para el Login 
    public class LoginDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    // CLASES AUXILIARES PARA LEER RANDOMUSER.ME
    // Necesarias para "mapear" la respuesta de la API externa
    public class RandomUserResponse
    {
        public List<Result> results { get; set; }
    }
    public class Result
    {
        public Name name { get; set; }
        public Location location { get; set; }
        public string email { get; set; }
        public string cell { get; set; }
    }
    public class Name { public string first { get; set; } public string last { get; set; } }
    public class Location { public string city { get; set; } public string country { get; set; } }


    class Program
    {
        static HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            // CONFIGURACIÓN:
            client.BaseAddress = new Uri("http://25.0.248.40:58765/api/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            Console.WriteLine("--- CLIENTE DE GESTIÓN DE TARJETAS (API REST) ---");
            Console.WriteLine("Conectando con el Servidor");
            // --- VERIFICACIÓN DE CONEXIÓN ---
            if (!await VerificarConexionServidor())
            {
                Console.WriteLine("\nPresiona cualquier tecla para salir...");
                Console.ReadKey();
                return; // Detiene el programa si no hay servidor
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n¡Conexión Exitosa con el Servidor!"); // Mensaje de éxito
            Console.ResetColor();

            Console.WriteLine("Presiona cualquier tecla para continuar al Login...");
            Console.ReadKey(); // <--- AQUÍ ESTÁ LA ESPERA QUE QUERÍAS

            Console.Clear();
            // ---------------------------------------------

            // MÉTODO PARA VERIFICAR SI EL SERVIDOR ESTÁ VIVO
            static async Task<bool> VerificarConexionServidor()
            {
                try
                {
                    Console.WriteLine($"Intentando conectar a: {client.BaseAddress}auth/ping");

                    // 1. CREAMOS UN CRONÓMETRO DE 5 SEGUNDOS (CancellationToken)
                    // Esto sustituye a modificar client.Timeout
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                    {
                        try
                        {
                            // 2. Enviamos el token junto con la petición
                            var response = await client.GetAsync("Auth/ping", cts.Token);

                            if (response.IsSuccessStatusCode)
                            {
                                return true;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"El servidor respondió error: {response.StatusCode}");
                                Console.ResetColor();
                                return false;
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            // Este bloque captura ESPECÍFICAMENTE cuando se acaban los 5 segundos
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n--- TIEMPO AGOTADO ---");
                            Console.WriteLine("El servidor no respondió en 5 segundos.");
                            Console.WriteLine("Posible causa: Firewall bloqueando o IP incorrecta.");
                            Console.ResetColor();
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n--- ERROR DE CONEXIÓN ---");
                    Console.WriteLine($"Detalle: {ex.Message}");
                    Console.ResetColor();
                    return false;
                }
            }


            // ---------------------------------------------------------
            // 1. LOGICA DE BLOQUEO / 3 INTENTOS
            // ---------------------------------------------------------
            // Llamamos a la función. Si retorna FALSE (porque fallaron los 3 intentos), cerramos todo.
            bool loginExitoso = await HandleLoginAsync();

            if (!loginExitoso)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nSe ha excedido el número de intentos o hubo un error. \nLa aplicación se cerrará.");
                Console.ResetColor();
                Console.ReadKey();
                return; // <--- AQUÍ SE SALE DEL PROGRAMA SI FALLA EL LOGIN
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Acceso concedido.\n");
            Console.ResetColor();

            // ---------------------------------------------------------
            // 2. BUCLE DEL MENÚ PRINCIPAL
            // ---------------------------------------------------------
            while (true)
            {
                DisplayMenu();
                string option = Console.ReadLine();

                try
                {
                    switch (option)
                    {
                        case "1":
                            // Aquí dentro implementamos la lógica de RandomUser
                            await InsertarSolicitudAsync();
                            break;
                        case "2":
                            await ConsultarSolicitudAsync();
                            break;
                        case "3":
                            await ModificarSolicitudAsync();
                            break;
                        case "4":
                            await EliminarSolicitudAsync();
                            break;
                        case "5":
                            Console.WriteLine("Cerrando sesión...");
                            return;
                        default:
                            Console.WriteLine("Opción no válida.");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\n--- ERROR DE COMUNICACIÓN ---");
                    Console.WriteLine($"Detalle: {e.Message}");
                }

                Console.WriteLine("\nPresiona Enter para continuar...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        // -------------------------------------------------------------------------
        // MÉTODOS CRUD
        // -------------------------------------------------------------------------

        // CASO 1: INSERTAR CON OPCIÓN DE RANDOM USER
        static async Task InsertarSolicitudAsync()
        {
            Console.WriteLine("\n--- NUEVA SOLICITUD ---");
            Console.WriteLine("1. Capturar datos manualmente");
            Console.WriteLine("2. Autocompletar con API Externa (RandomUser)");
            Console.Write("Seleccione una opción: ");
            string modo = Console.ReadLine();

            ApiSolicitud nuevaSolicitud = new ApiSolicitud();

            if (modo == "2")
            {
                Console.WriteLine("Obteniendo datos de randomuser.me...");
                try
                {
                    // Lógica para consumir API Externa
                    // Usamos una URL absoluta, HttpClient la maneja aunque tenga BaseAddress definida
                    var randomData = await client.GetFromJsonAsync<RandomUserResponse>("https://randomuser.me/api/");

                    if (randomData != null && randomData.results.Count > 0)
                    {
                        var user = randomData.results[0];

                        // Mapeamos los datos de RandomUser a tu clase ApiSolicitud
                        nuevaSolicitud.NombreCliente = $"{user.name.first} {user.name.last}";
                        nuevaSolicitud.Direccion = $"{user.location.city}, {user.location.country}";
                        nuevaSolicitud.Correo = user.email;
                        nuevaSolicitud.TelefonoCelular = user.cell;
                        nuevaSolicitud.Ocupacion = "Desconocida (Editar)"; // RandomUser no tiene dato

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("\n--- DATOS OBTENIDOS ---");
                        Console.WriteLine($"Nombre:   {nuevaSolicitud.NombreCliente}");
                        Console.WriteLine($"Dirección:{nuevaSolicitud.Direccion}");
                        Console.WriteLine($"Correo:   {nuevaSolicitud.Correo}");
                        Console.WriteLine($"Celular: {nuevaSolicitud.TelefonoCelular}");
                        Console.WriteLine("-----------------------");
                        Console.ResetColor();

                        Console.Write("¿Desea usar estos datos? (S = Sí, usar / N = No, capturar manual): ");
                        if (Console.ReadLine().ToUpper() != "S")
                        {
                            // Si dice que no, limpiamos para captura manual
                            nuevaSolicitud = new ApiSolicitud();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al consultar RandomUser: {ex.Message}. Se usará captura manual.");
                    nuevaSolicitud = new ApiSolicitud();
                }
            }

            // TERMINAR DE LLENAR (O EDITAR) LOS DATOS
            // Si vino de RandomUser, los Console.ReadLine mostrarán el valor precargado si quisieras hacer un método de edición complejo,
            // pero para simplificar, pediremos el FOLIO (que es obligatorio) y permitiremos editar el resto si está vacío.

            Console.Write("Ingrese FOLIO (Obligatorio): ");
            nuevaSolicitud.Folio = Console.ReadLine();

            if (string.IsNullOrEmpty(nuevaSolicitud.NombreCliente))
            {
                Console.Write("Nombre: ");
                nuevaSolicitud.NombreCliente = Console.ReadLine();
            }
            if (string.IsNullOrEmpty(nuevaSolicitud.Direccion))
            {
                Console.Write("Dirección: ");
                nuevaSolicitud.Direccion = Console.ReadLine();
            }
            if (string.IsNullOrEmpty(nuevaSolicitud.TelefonoCelular))
            {
                Console.Write("Teléfono: ");
                nuevaSolicitud.TelefonoCelular = Console.ReadLine();
            }
            if (string.IsNullOrEmpty(nuevaSolicitud.Correo))
            {
                Console.Write("Correo: ");
                nuevaSolicitud.Correo = Console.ReadLine();
            }

            // La ocupación siempre pedimos revisarla
            Console.Write($"Ocupación [Actual: {nuevaSolicitud.Ocupacion ?? "Vacia"}]: ");
            string nuevaOcup = Console.ReadLine();
            if (!string.IsNullOrEmpty(nuevaOcup)) nuevaSolicitud.Ocupacion = nuevaOcup;
            if (string.IsNullOrEmpty(nuevaSolicitud.Ocupacion)) nuevaSolicitud.Ocupacion = "Sin Especificar";

            // EVALUACIÓN DE CRÉDITO
            string status = "Rechazado";
            decimal limite = 0;
            Console.Write("¿Aprobar solicitud? (S/N): ");
            if (Console.ReadLine().ToUpper() == "S")
            {
                status = "Aprobado";
                Console.Write("Ingrese Límite de Crédito: ");
                decimal.TryParse(Console.ReadLine(), out limite);
            }
            nuevaSolicitud.StatusSolicitud = status;
            nuevaSolicitud.LimiteCredito = limite;
            nuevaSolicitud.FechaSolicitud = DateTime.Now;

            // ENVIAR A TU API LOCAL
            Console.WriteLine("Enviando a la base de datos...");
            var response = await client.PostAsJsonAsync("Solicitudes", nuevaSolicitud);

            if (response.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("¡Solicitud registrada con éxito!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error al guardar: {response.StatusCode}");
            }
            Console.ResetColor();
        }

        // CASO 2: CONSULTAR
        // Asegúrate de tener: using System.Text.Json; al inicio

        static async Task ConsultarSolicitudAsync()
        {
            Console.Write("Folio a consultar: ");
            string folio = Console.ReadLine();

            try
            {
                // Descargar como String para ver que llega realmente
                var response = await client.GetAsync($"Solicitudes/{folio}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error del servidor: {response.StatusCode}");
                    Console.ResetColor();
                    return;
                }

                string jsonRespuesta = await response.Content.ReadAsStringAsync();

                // PASO 2:Imprimimos lo que llego
                //Console.ForegroundColor = ConsoleColor.DarkGray;
                //Console.WriteLine($"\n[DEBUG] JSON RECIBIDO: {jsonRespuesta}\n");
                

                // PASO 3: Intentamos convertir manualmente con opciones flexibles
                var opciones = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // Ignora mayúsculas/minúsculas
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString // Permite leer números que vengan como texto "100"
                };

                var solicitud = System.Text.Json.JsonSerializer.Deserialize<ApiSolicitud>(jsonRespuesta, opciones);

                // PASO 4: Mostrar datos
                if (solicitud != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n--- DATOS DEL CLIENTE ---");
                    Console.WriteLine($"Folio:              {solicitud.Folio}");
                    Console.WriteLine($"Nombre:             {solicitud.NombreCliente}");
                    Console.WriteLine($"Direccion:          {solicitud.Direccion}");
                    Console.WriteLine($"Celular:            {solicitud.TelefonoCelular}");
                    Console.WriteLine($"Status:             {solicitud.StatusSolicitud}");

                    // Usamos HasValue para saber si es nulo
                    if (solicitud.LimiteCredito.HasValue && solicitud.LimiteCredito.Value > 0)
                    {
                        Console.WriteLine($"Límite de Credito:   {solicitud.LimiteCredito.Value:C}");
                        Console.WriteLine($"Numero Tarjeta:      {solicitud.NumTarjeta}");
                        Console.WriteLine($"Fecha de Corte:      {solicitud.FechaCorte.Value.ToShortDateString()}");
                        Console.WriteLine($"Clabe SPEI:          {solicitud.ClabeSPEI}");
                    }
                    else
                    {
                        Console.WriteLine($"Límite:     (Sin Asignar)");
                    }

                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n--- ERROR AL PROCESAR JSON ---");
                Console.WriteLine($"Mensaje: {ex.Message}");
                Console.WriteLine("Esto suele pasar si tienes duplicada la clase ApiSolicitud o no se recompiló.");
                Console.ResetColor();
            }
        }

        // CASO 3: MODIFICAR
        static async Task ModificarSolicitudAsync()
        {
            Console.Write("Folio a modificar: ");
            string folio = Console.ReadLine();

            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                // 1. Obtener datos actuales
                var sol = await client.GetFromJsonAsync<ApiSolicitud>($"Solicitudes/{folio}");

                Console.WriteLine($"Modificando a: {sol.NombreCliente}");

                // 2. Pedir nuevos datos (Enter para mantener actual)
                Console.Write("Nuevo Folio (Enter para saltar): ");
                string val = Console.ReadLine();
                if (!string.IsNullOrEmpty(val)) sol.Folio = val;
                
                Console.Write("Nuevo Nombre (Enter para saltar): ");
                val = Console.ReadLine();
                if (!string.IsNullOrEmpty(val)) sol.NombreCliente = val;

                Console.Write("Nueva Dirección (Enter para saltar): ");
                val = Console.ReadLine();
                if (!string.IsNullOrEmpty(val)) sol.Direccion = val;

                Console.Write("Nuevo Numero Celular (Enter para saltar): ");
                val = Console.ReadLine();
                if (!string.IsNullOrEmpty(val)) sol.TelefonoCelular = val;

                Console.Write("Nuevo Correo (Enter para saltar): ");
                val = Console.ReadLine();
                if (!string.IsNullOrEmpty(val)) sol.Correo = val;

                Console.Write("Nueva Ocupacion (Enter para saltar): ");
                val = Console.ReadLine();
                if (!string.IsNullOrEmpty(val)) sol.Ocupacion = val;
                Console.ResetColor();

                // 3. Enviar actualización
                var response = await client.PutAsJsonAsync($"Solicitudes", sol);

                if (response.IsSuccessStatusCode)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Actualizado correctamente.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {response.StatusCode}");
                    Console.ResetColor();
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Folio no encontrado.");
                Console.ResetColor();
            }
        }

        // CASO 4: ELIMINAR
        static async Task EliminarSolicitudAsync()
        {
            Console.Write("Folio a eliminar: ");
            string folio = Console.ReadLine();
            Console.Write("¿Confirmar? (S/N): ");
            if (Console.ReadLine().ToUpper() == "S")
            {
                var response = await client.DeleteAsync($"Solicitudes/{folio}");
                if (response.IsSuccessStatusCode)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Eliminado.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {response.StatusCode}");
                    Console.ResetColor();
                } 
            }
        }

        // -------------------------------------------------------------------------
        // LOGIN (AQUÍ ESTÁ LA LÓGICA DE LOS 3 INTENTOS)
        // -------------------------------------------------------------------------
        static async Task<bool> HandleLoginAsync()
        {
            Console.Clear();
            Console.WriteLine("--- LOGIN ---");

            // ESTE CICLO CONTROLA LOS 3 INTENTOS
            for (int attempts = 0; attempts < 3; attempts++)
            {
                Console.Write($"Usuario (Intento {attempts + 1}/3): ");
                string username = Console.ReadLine();
                Console.Write("Contraseña: ");
                string password = Console.ReadLine();

                var loginData = new LoginDTO { Username = username, Password = password };

                try
                {
                    // Enviamos credenciales a la API
                    var response = await client.PostAsJsonAsync("Auth/login", loginData);

                    if (response.IsSuccessStatusCode)
                    {
                        return true; // ÉXITO: Sale del método y devuelve true
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Credenciales incorrectas.");
                        Console.ResetColor();
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error de conexión con el servidor: {ex.Message}");
                    // Si no hay servidor, podríamos querer salir inmediatamente o reintentar
                    // Aquí decidimos contar como intento fallido.
                    Console.ResetColor();
                }
            }

            // Si el ciclo for termina sin retornar true, significa que falló 3 veces
            return false;
        }

        static void DisplayMenu()
        {
            Console.WriteLine("\n--- MENÚ ---");
            Console.WriteLine("1. Insertar Solicitud");
            Console.WriteLine("2. Consultar Solicitud");
            Console.WriteLine("3. Modificar Solicitud");
            Console.WriteLine("4. Eliminar Solicitud");
            Console.WriteLine("5. Salir");
            Console.Write("Opción: ");
        }
    }
}




/* este es codigo que funciona con SOCKETS entonces se refactorizaron algunas
de las funcionalidades como asincronas para probar que funciona por ejemplo 
en esta version solo funciona el login y el post lo demas esta descomentado y usado para usarse


using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Net.Http.Json; // Necesitas este using
using System;
using System.Threading.Tasks;

// El namespace del proyecto Cliente
namespace IntegradoraTarjCliente
{

    public class LoginDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    class Program
    {
        // Instancia del Gestor, que maneja Sockets, JSON y lógica de conexión.
        // Se asume que este archivo hace referencia a las clases ClienteGestor, Solicitud, y RespuestaServidor.
        private static ClienteGestor Gestor = new ClienteGestor();

        static HttpClient client = new HttpClient();

        //Cuando se usa sockets --- static void Main(string[] args) 
        static async Task Main(string[] args)
        {
            client.BaseAddress = new Uri("http://25.0.248.40:58765/api/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            //Console.WriteLine("--- CLIENTE DE GESTIÓN DE TARJETAS (SOCKETS) ---");
            Console.WriteLine("--- CLIENTE DE GESTIÓN DE TARJETAS (API REST) ---");

            // 1. Validar la conexión del servidor al inicio (delegada a ClienteGestor)
            //if (!Gestor.CheckServerConnection())
           // {
           //     Console.WriteLine("\nPresione cualquier tecla para salir...");
            //    Console.ReadKey();
            //    return; // Salir si el servidor no está disponible
           // }
            //Validacion sockets if (!HandleLogin())
            if (!await HandleLoginAsync())
            {
                Console.WriteLine("\nAutenticación fallida. Cerrando aplicación.");
                Console.ReadKey(); 
                return; //Detiene la aplicacion si HandleLogin retorna false
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
                        // case 1 con socketscase "1":
                         //   request = BuildInsertRequest();
                         //   break;
                        case "1":
                            // PASO A: Capturar los datos en el objeto
                            ApiSolicitud nuevaSolicitud = CapturarDatosParaApi();

                            Console.WriteLine("Enviando datos a la API...");

                            try
                            {
                                // PASO B: Enviar usando PostAsJsonAsync
                                // IMPORTANTE: Reemplaza "Solicitudes" con el nombre real de tu controlador en la API
                                // Si tu controlador es [Route("api/[controller]")] y la clase es SolicitudesController, pon "Solicitudes"
                                var response = await client.PostAsJsonAsync("Solicitudes", nuevaSolicitud);

                                if (response.IsSuccessStatusCode)
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("¡ÉXITO! Solicitud guardada en SQL Server.");
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"ERROR: La API respondió {response.StatusCode}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error de conexión: {ex.Message}");
                            }
                            Console.ResetColor();
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

        //static bool HandleLogin()
        //{
        //    Console.Clear();
        //    Console.WriteLine("--- BIENVENIDO AL SISTEMA DE SOLICITUD DE TARJETA DE CREDITO DEL BANCO DE MEXICO ---");
         //   Console.WriteLine("-- INICIO DE SESION ---");

            //// Intentar hasta 3 veces
         //   for (int attempts = 0; attempts < 3; attempts++)
            //{
              //  Console.Write("Usuario: ");
              //  string username = Console.ReadLine();
              //  Console.Write("Contraseña: ");
               // string password = Console.ReadLine();

                // 1. Construir la solicitud de Login
                //string request = $"LOGIN|{username};{password}";

                // 2. Enviar solicitud y recibir respuesta JSON
                //string responseJson = Gestor.SendRequest(request);

                // 3. Deserializar la respuesta
                //RespuestaServidor respuesta = null;
                //try
                //{
                  //  respuesta = Newtonsoft.Json.JsonConvert.DeserializeObject<RespuestaServidor>(responseJson);
                //}
                //catch (Newtonsoft.Json.JsonException)
                //{
                    // Manejar si el Servidor devolvió algo que no es JSON (ej., error de la propia red)
                    //Console.WriteLine("Error: Respuesta de formato JSON inválido.");
                  //  continue;
                //}

                //if (respuesta != null && respuesta.Exito)
                //{
                  //  return true; // Aqui se genera el acceso
                //}
                //else
               // {
                    //Console.ForegroundColor = ConsoleColor.Red;
                  //  Console.WriteLine($"Intento {attempts + 1} fallido: {respuesta?.Mensaje ?? "Error de comunicación."}");
                //    Console.ResetColor();
              //  }
            //}
           // return false; // Bloquear si falla después de 3 intentos
       // } //funcion con socket

        // Cambiamos a 'async Task<bool>' porque HttpClient es asíncrono
        static async Task<bool> HandleLoginAsync()
        {
            Console.Clear();
            Console.WriteLine("--- BIENVENIDO ---");
            Console.WriteLine("-- INICIO DE SESION (API) ---");

            for (int attempts = 0; attempts < 3; attempts++)
            {
                Console.Write("Usuario: ");
                string username = Console.ReadLine();
                Console.Write("Contraseña: ");
                string password = Console.ReadLine();

                // 1. Crear el objeto DTO (usando la clase que pusiste arriba)
                var loginData = new LoginDTO { Username = username, Password = password };

                try
                {
                    // 2. Enviar a la API
                    // Asumiendo que tu controlador se llama AuthController y el método login
                    var response = await client.PostAsJsonAsync("Auth/login", loginData);

                    if (response.IsSuccessStatusCode)
                    {
                        // Opcional: Podrías leer el token o rol aquí si lo necesitas
                        return true;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: Usuario o contraseña incorrectos. (Intento {attempts + 1}/3)");
                        Console.ResetColor();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"No se pudo conectar al servidor: {ex.Message}");
                    return false; // Si no hay servidor, salimos
                }
            }
            return false;
        }
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

        static ApiSolicitud CapturarDatosParaApi()
        {
            Console.WriteLine("\n--- NUEVA SOLICITUD (API) ---");

            // 1. Pedimos los datos (Copia exacta de tu lógica anterior)
            Console.Write("Folio: ");
            string folio = Console.ReadLine();
            Console.Write("Nombre: ");
            string nombre = Console.ReadLine();
            Console.Write("Dirección: ");
            string direccion = Console.ReadLine();
            Console.Write("Teléfono: ");
            string telefono = Console.ReadLine();
            Console.Write("Correo: ");
            string correo = Console.ReadLine();
            Console.Write("Ocupación: ");
            string ocupacion = Console.ReadLine();

            // 2. Lógica de Aprobación (Reutilizada)
            string status = "Rechazado";
            decimal limite = 0;

            Console.Write("¿Aprobar solicitud? (S/N): ");
            if (Console.ReadLine().ToUpper() == "S")
            {
                status = "Aprobado";
                Console.Write("Ingrese Límite de Crédito: ");
                decimal.TryParse(Console.ReadLine(), out limite);
            }

            // 3. RETORNO: Aquí está la magia. Devolvemos el OBJETO, no texto.
            return new ApiSolicitud
            {
                Folio = folio,
                NombreCliente = nombre, // Asegúrate que coincida con ApiSolicitud.cs
                Direccion = direccion,
                TelefonoCelular = telefono,
                Correo = correo,
                Ocupacion = ocupacion,
                StatusSolicitud = status,
                LimiteCredito = limite,
                FechaSolicitud = DateTime.Now
            };
        }
    }
}*/
