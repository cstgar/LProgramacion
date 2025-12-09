using System;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;          
using System.Net.Http.Json;     
using System.Threading.Tasks;   // <--- Para tareas asíncronas
using System.Collections.Generic;
using System.Linq;

namespace API_TarjetasCredito
{
    // Renombramos la clase a 'Solicitud' (singular)
    public class Solicitud
    {
        // === Datos de la Solicitud (NO NULL en la DB) ===
        public string Folio { get; set; }
        public string NombreCliente { get; set; }
        public string Direccion { get; set; }
        public string TelefonoCelular { get; set; }
        public string Correo { get; set; }
        public string Ocupacion { get; set; }
        public string StatusSolicitud { get; set; }

        // Propiedad para la fecha de la solicitud
        public DateTime FechaSolicitud { get; set; }


        // === Datos de la Tarjeta (NULLABLES en la DB) ===

        // Usamos el signo de interrogación (?) en decimal y DateTime para permitir NULLS
        public decimal? LimiteCredito { get; set; }
        public string? NumTarjeta { get; set; }
        public DateTime? FechaCorte { get; set; }
        public string? ClabeSPEI { get; set; }
    }

    
}