using Microsoft.AspNetCore.Mvc;
using API_TarjetasCredito;

namespace API_TarjetasCredito.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private ConexionDB _conexion;

        public AuthController()
        {
            // Inicializamos tu clase de conexión a SQL
            _conexion = new ConexionDB();
        }

        //GET: api/auth/ping
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("Conexion exitosa");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel datosIngresados)
        {
            // 1. Validar el Modelo Gracias a los [Required] en LoginModel
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Devuelve error 400 si falta usuario o pass
            }

            try
            {
                // 2. Usar tu clase ConexionDB para buscar en SQL Server
                Usuario usuarioEncontrado = _conexion.Autenticar(datosIngresados.Username, datosIngresados.Password);

                // 3. Verificar si se encontró
                if (usuarioEncontrado != null)
                {
                    return Ok(new
                    {
                        Mensaje = "Login exitoso",
                        Usuario = usuarioEncontrado.Username,
                        Rol = usuarioEncontrado.Rol
                    });
                }
                else
                {
                    return Unauthorized("Usuario o contraseña incorrectos");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}