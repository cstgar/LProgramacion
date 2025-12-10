using System;

namespace IntegradoraTarjCliente
{
    public class ApiSolicitud
    {
        public string Folio { get; set; }
        public string NombreCliente { get; set; }
        public string Direccion { get; set; }
        public string TelefonoCelular { get; set; }
        public string Correo { get; set; }
        public string Ocupacion { get; set; }
        public string StatusSolicitud { get; set; }
        public decimal? LimiteCredito { get; set; }
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;
        public string? NumTarjeta { get; set; }
        public DateTime? FechaCorte { get; set; }
        public string? ClabeSPEI { get; set; }
    }
}
