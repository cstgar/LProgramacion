using Microsoft.AspNetCore.Mvc;
using API_TarjetasCredito;
using System.Linq;
using System.Text.Json;

namespace API_TarjetasCredito.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Esto define la URL como: http://... /api/solicitudes
    public class SolicitudesController : ControllerBase
    {
        // Instancia de tu lógica de negocio existente
        private readonly GestorTarjetas _gestor = new GestorTarjetas();

        // 1. Endpoint para CONSULTAR (GET)
        // Se llama así: GET /api/solicitudes/{folio}
        [HttpGet("{folio}")]
        public IActionResult Consultar(string folio)
        {
            Console.WriteLine("-------------------------");
            Console.WriteLine("LLego una nueva peticion GET");
            string datosTexto = JsonSerializer.Serialize(folio);
            Console.Write($"[DATOS RECIBIDOS GET]: {datosTexto} ");
            Console.WriteLine("-------------------------");

            var respuesta = _gestor.ConsultarSolicitud(folio);

            if (respuesta.Exito && respuesta.Solicitudes != null)
            {
                return Ok(respuesta.Solicitudes.FirstOrDefault()); // Retorna 200 OK con el objeto JSON
            }
            return NotFound(new { mensaje = respuesta.Mensaje }); // Retorna 404 si no existe
        }

        // 2. Endpoint para INSERTAR (POST)
        // Se llama así: POST /api/solicitudes (con JSON en el cuerpo)
        [HttpPost]
        public IActionResult Insertar([FromBody] Solicitud nuevaSolicitud)
        {
            Console.WriteLine("-------------------------");
            Console.WriteLine("LLego una nueva peticion POST");
            string datosTexto = JsonSerializer.Serialize(nuevaSolicitud);
            Console.Write($"[DATOS RECIBIDOS POST]: {datosTexto} ");
            Console.WriteLine("-------------------------");

            var respuesta = _gestor.ProcesarNuevaSolicitud(nuevaSolicitud);

            if (respuesta.Exito)
            {
                return StatusCode(201, new { mensaje = respuesta.Mensaje }); // Retorna 201 Created
            }
            return BadRequest(new { mensaje = respuesta.Mensaje }); // Retorna 400 si falla
        }

        // ----------------------------------------------------
        // 3. PUT: Modificar
        // Se llama así: PUT /api/solicitudes (con JSON en el cuerpo)
        // ----------------------------------------------------
        [HttpPut]
        public IActionResult Modificar([FromBody] Solicitud solicitudModificar)
        {
            Console.WriteLine("-------------------------");
            Console.WriteLine("LLego una nueva peticion PUT");
            string datosTexto = JsonSerializer.Serialize(solicitudModificar);
            Console.Write($"[DATOS RECIBIDOS PUT]: {datosTexto} ");
            Console.WriteLine("-------------------------");

            // Llamamos a tu lógica de negocio existente: ProcesarModificacion
            var respuesta = _gestor.ProcesarModificacion(solicitudModificar);

            if (respuesta.Exito)
            {
                return Ok(new { mensaje = respuesta.Mensaje });
            }
            // Si falla (ej. folio no existe), devolvemos 404 o 400
            return NotFound(new { mensaje = respuesta.Mensaje });
        }

        // ----------------------------------------------------
        // 4. DELETE: Eliminar
        // Se llama asi: DELETE /api/{folio}
        // ----------------------------------------------------
        [HttpDelete("{folio}")]
        public IActionResult Eliminar(string folio)
        {
            Console.WriteLine("-------------------------");
            Console.WriteLine("LLego una nueva peticion DELETE");
            string datosTexto = JsonSerializer.Serialize(folio);
            Console.Write($"[DATOS RECIBIDOS DELETE]: {datosTexto} ");
            Console.WriteLine("-------------------------");

            // Llamamos a tu lógica de negocio existente: ProcesarEliminacion
            var respuesta = _gestor.ProcesarEliminacion(folio);

            if (respuesta.Exito)
            {
                // El estándar REST para borrar es devolver 204 No Content (éxito sin cuerpo)
                // O devolver Ok con un mensaje.
                return Ok(new { mensaje = respuesta.Mensaje });
            }
            return NotFound(new { mensaje = respuesta.Mensaje });
        }

        // ----------------------------------------------------
        // 5. NUEVO: Simular Cliente (API Externa)
        // ----------------------------------------------------
        [HttpGet("simular-cliente")]
        public async Task<IActionResult> ObtenerUsuarioExterno()
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    // 1. Hacemos la petición a la URL de RandomUser
                    var respuestaApi = await httpClient.GetFromJsonAsync<RespuestaRandomUser>("https://randomuser.me/api/");

                    // 2. Tomamos el primer resultado de la lista
                    var usuario = respuestaApi?.results.FirstOrDefault();

                    if (usuario != null)
                    {
                        // 3. MAPEO: Convertimos los datos externos a TU formato
                        var nuevaSolicitud = new Solicitud
                        {
                            // Generamos un folio temporal
                            Folio = new Random().Next(1000, 9999).ToString(),

                            // Unimos nombre y apellido
                            NombreCliente = $"{usuario.name.first} {usuario.name.last}",

                            Correo = usuario.email,
                            TelefonoCelular = usuario.phone,
                            Direccion = $"{usuario.location.city}, {usuario.location.country}",
                            Ocupacion = "Cliente Simulado",
                            StatusSolicitud = "Captura", // Status inicial

                            // Llenamos datos técnicos por defecto para evitar errores
                            LimiteCredito = 0,
                            NumTarjeta = "N/A",
                            ClabeSPEI = "N/A",

                            // Ponemos la fecha de hoy
                            FechaSolicitud = DateTime.Now,
                            FechaCorte = DateTime.Now.AddDays(30)
                        };

                        return Ok(nuevaSolicitud);
                    }

                    return NotFound("No se pudo obtener datos de la API externa.");
                }
                catch (Exception ex)
                {
                    // Si no tienes internet o falla la API
                    return StatusCode(500, $"Error de conexión: {ex.Message}");
                }
            }
        }
    }

    // ==========================================
    // Clases para leer la API de RandomUser
    // ==========================================
    public class RespuestaRandomUser
    {
        public List<UsuarioRandom> results { get; set; }
    }

    public class UsuarioRandom
    {
        public Nombre name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public Localizacion location { get; set; }
    }

    public class Nombre
    {
        public string first { get; set; }
        public string last { get; set; }
    }

    public class Localizacion
    {
        public string city { get; set; }
        public string country { get; set; }
    }
}