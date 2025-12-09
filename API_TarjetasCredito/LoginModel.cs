using System.ComponentModel.DataAnnotations;

namespace API_TarjetasCredito
{
    public class LoginModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string Username { get; set; }
        [Required(ErrorMessage = "La contrasenia es obligatoria")]
        public string Password { get; set; }
    }
}
