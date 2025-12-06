using IntegradoraTarjCliente;
using System.Collections.Generic;

namespace IntegradoraTarjCliente
{
    public class RespuestaServidor
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }

        // Se usa para devolver los resultados de CONSULTAR
        public List<Solicitud> Solicitudes { get; set; }
    }
}