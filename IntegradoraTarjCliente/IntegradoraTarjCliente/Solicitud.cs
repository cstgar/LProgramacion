using System;

namespace IntegradoraTarjCliente
{
    public class Solicitud
    {
        public string Folio { get; set; }
        public string NombreCliente { get; set; }
        public string Direccion { get; set;}
        public string TelefonoCelular { get; set; }
        public string Correo { get; set;}
        public string Ocupacion { get; set;}
        public string StatusSolicitud { get; set; }

        //Propiedades para la fecha de la solicitud
        public DateTime FechaSolicitud { get; set;}
        //Datos de la Tarjeta (NULLABLES en la DB) ===
        //Se usa el signo de interrogacion (?) en decimal y DateTime para permitir NULLS
        public decimal? LimiteCredito { get; set;}
        public string NumTarjeta { get; set;}
        public DateTime? FechaCorte { get; set;}
        public string ClabeSPEI { get; set;}

    }
}
