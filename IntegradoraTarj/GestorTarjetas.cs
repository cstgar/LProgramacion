using System;
using System.Collections.Generic;

namespace SocketServidor
{
    public class GestorTarjetas
    {
        // El Gestor contiene la lógica de negocio y llama a la capa de datos.
        private readonly ConexionDB _db = new ConexionDB();

        // Constructor: Puedes inicializar aquí la capa de datos si fuera necesario
        public GestorTarjetas() { }

        // --- Funciones Auxiliares
        private static string GenerateCardNumber()
        {
            Random r = new Random();
            return $"4{r.Next(1000, 9999)}{r.Next(1000, 9999)}{r.Next(1000, 9999)}{r.Next(1000, 9999)}";
        }

        private static string GenerateClabe()
        {
            Random r = new Random();
            return $"{r.Next(100, 999)}{r.Next(100, 999)}{r.Next(100, 999)}{r.Next(1000, 9999)}{r.Next(100, 999)}";
        }

        // --------------------------------------------------------
        // MÉTODOS DE PROCESAMIENTO CENTRAL (Reemplazan HandleX)
        // --------------------------------------------------------

        public RespuestaServidor ProcesarNuevaSolicitud(Solicitud s)
        {
            // Lógica de aprobación/rechazo y asignación de datos sensibles
            if (s.StatusSolicitud == "Aprobado")
            {
                // El servidor asigna los datos al objeto Solicitud
                s.NumTarjeta = GenerateCardNumber();
                s.FechaCorte = DateTime.Today.AddDays(25);
                s.ClabeSPEI = GenerateClabe();
                // Otras validaciones de límites, si es necesario, irían aquí.
            }

            // Llama a la capa de datos
            bool exito = _db.Insertar(s);

            return new RespuestaServidor
            {
                Exito = exito,
                Mensaje = exito ? "Registro insertado con exito." : "Error: No se pudo insertar en la BD (Folio duplicado?).",
                Solicitudes = exito ? new List<Solicitud> { s } : null
            };
        }

        public RespuestaServidor ConsultarSolicitud(string folio)
        {
            Solicitud s = _db.Consultar(folio);

            return new RespuestaServidor
            {
                Exito = s != null,
                Mensaje = s != null ? "Consulta exitosa." : "Folio no encontrado.",
                Solicitudes = s != null ? new List<Solicitud> { s } : null
            };
        }
        public RespuestaServidor ProcesarModificacion(Solicitud s)
        {
            // Nota: Aquí se podría agregar lógica de negocio adicional (ej. registrar historial de cambios)

            bool exito = _db.Actualizar(s); // Llama al método UPDATE de ConexionDB.cs

            return new RespuestaServidor
            {
                Exito = exito,
                Mensaje = exito
                    ? $"Registro con Folio {s.Folio} modificado con exito."
                    : $"Error: No se pudo modificar el registro (Folio no encontrado o error de BD)."
            };
        }
        public RespuestaServidor ProcesarEliminacion(string folio)
        {
            if (string.IsNullOrWhiteSpace(folio))
            {
                return new RespuestaServidor { Exito = false, Mensaje = "Debe proporcionar un Folio para eliminar." };
            }

            bool exito = _db.Eliminar(folio); // Llama al método DELETE de ConexionDB.cs

            return new RespuestaServidor
            {
                Exito = exito,
                Mensaje = exito
                    ? $"Registro con Folio {folio} eliminado con exito."
                    : $"Error: No se encontró ningún registro con Folio {folio} para eliminar."
            };
        }
    }
}