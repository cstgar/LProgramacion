using System.Collections.Generic;

namespace SocketServidor
{
    // Clase que el Servidor enviará al Cliente (usando JSON)
    public class RespuestaServidor
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }

        // La propiedad 'Datos' puede contener una lista de objetos Solicitud
        // (útil para la operación CONSULTAR)
        public List<Solicitud> Solicitudes { get; set; }
    }
}
