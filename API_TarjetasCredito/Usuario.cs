namespace API_TarjetasCredito
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // En un proyecto real, usarías PasswordHash
        public string Rol { get; set; } // Administrador, Cajero, etc.
    }
}