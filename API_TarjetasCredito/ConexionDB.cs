using Microsoft.Data.SqlClient;
using System.Data;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace API_TarjetasCredito
{
    public class ConexionDB
    {
        // NOTA: La cadena de conexión se mueve aquí desde Program.cs
        private const string ConnectionString = "Data Source=25.0.248.40; Database=lp_abc_multi; User Id=access_cinthia; Password=Salome123; TrustServerCertificate=True;";

        // Constructor
        public ConexionDB() { }

        public Usuario Autenticar(string username, string password)
        {
            // Lógica para buscar el usuario y validar la contraseña en la BD
            using (SqlConnection conexion = new SqlConnection(ConnectionString))
            {
                conexion.Open();

                // Referencia a la tabla llamada 'Usuarios'
                string sql = "SELECT Id, Username, Password, Rol FROM Usuarios WHERE Username = @User AND Password = @Pass";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.Add("@User", SqlDbType.VarChar, 50).Value = username;
                    cmd.Parameters.Add("@Pass", SqlDbType.VarChar, 50).Value = password;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Mapear los datos del lector al objeto Usuario
                            return new Usuario
                            {
                                Id = (int)reader["Id"],
                                Username = reader["Username"].ToString(),
                                Rol = reader["Rol"].ToString()
                                // No es necesario devolver la contraseña
                            };
                        }
                        return null; // Credenciales inválidas
                    }
                }
            }
        }

        // Mapea los resultados del SqlDataReader al objeto Solicitud (auxiliar)
        private Solicitud MapearSolicitud(SqlDataReader reader)
        {
            return new Solicitud
            {
                // === CAMPOS NO NULLABLES ===
                Folio = reader["Folio"] != DBNull.Value ? reader["Folio"].ToString() : string.Empty,
                NombreCliente = reader["NombreCliente"] != DBNull.Value ? reader["NombreCliente"].ToString() : string.Empty,

                //MAPEO COMPLETO DE CAMPOS NO NULL
                Direccion = reader["Direccion"] != DBNull.Value ? reader["Direccion"].ToString() : string.Empty,
                TelefonoCelular = reader["TelefonoCelular"] != DBNull.Value ? reader["TelefonoCelular"].ToString() : string.Empty,
                Correo = reader["Correo"] != DBNull.Value ? reader["Correo"].ToString() : string.Empty,
                Ocupacion = reader["Ocupacion"] != DBNull.Value ? reader["Ocupacion"].ToString() : string.Empty,
                // ----------------------------------------------------

                StatusSolicitud = reader["StatusSolicitud"].ToString(),

                // === CAMPOS NULLABLES (Mapeo que ya estaba bien) ===
                LimiteCredito = reader["LimiteCredito"] != DBNull.Value ? (decimal?)reader["LimiteCredito"] : null,
                NumTarjeta = reader["NumTarjeta"] != DBNull.Value ? reader["NumTarjeta"].ToString() : null,
                FechaCorte = reader["FechaCorte"] != DBNull.Value ? (DateTime?)reader["FechaCorte"] : null,
                ClabeSPEI = reader["ClabeSPEI"] != DBNull.Value ? reader["ClabeSPEI"].ToString() : null,
            };
        }

        // --- MÉTODOS CRUD ---

        public bool Insertar(Solicitud s)
        {
            // Implementación refactorizada de HandleInsert
            using (SqlConnection conexion = new SqlConnection(ConnectionString))
            {
                conexion.Open();
                string sql = @"
                    INSERT INTO Solicitudes (Folio, NombreCliente, Direccion, TelefonoCelular, Correo, Ocupacion, StatusSolicitud, NumTarjeta, LimiteCredito, FechaCorte, ClabeSPEI)
                    VALUES (@Folio, @Nombre, @Dir, @Tel, @Correo, @Ocupacion, @Status, @NumTarjeta, @Limite, @FechaCorte, @Clabe);";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    // Asignación de parámetros usando las propiedades del objeto Solicitud
                    cmd.Parameters.Add("@Folio", SqlDbType.VarChar, 50).Value = s.Folio;
                    cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = s.NombreCliente;
                    cmd.Parameters.Add("@Dir", SqlDbType.VarChar, 255).Value = s.Direccion;
                    cmd.Parameters.Add("@Tel", SqlDbType.VarChar, 20).Value = s.TelefonoCelular;
                    cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value = s.Correo;
                    cmd.Parameters.Add("@Ocupacion", SqlDbType.VarChar, 50).Value = s.Ocupacion;
                    cmd.Parameters.Add("@Status", SqlDbType.VarChar, 20).Value = s.StatusSolicitud;

                    // Manejo de valores NULLables (usa el operador de coalescencia null ??)
                    cmd.Parameters.Add("@NumTarjeta", SqlDbType.VarChar, 20).Value = (object)s.NumTarjeta ?? DBNull.Value;
                    cmd.Parameters.Add("@Limite", SqlDbType.Decimal).Value = (object)s.LimiteCredito ?? DBNull.Value;
                    cmd.Parameters.Add("@FechaCorte", SqlDbType.Date).Value = (object)s.FechaCorte ?? DBNull.Value;
                    cmd.Parameters.Add("@Clabe", SqlDbType.VarChar, 50).Value = (object)s.ClabeSPEI ?? DBNull.Value;

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public Solicitud Consultar(string folio)
        {
            // Implementación refactorizada de HandleQuery
            using (SqlConnection conexion = new SqlConnection(ConnectionString))
            {
                conexion.Open();
                string sql = "SELECT * FROM Solicitudes WHERE Folio = @Folio";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.Add("@Folio", SqlDbType.VarChar, 50).Value = folio;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Usa la función auxiliar para mapear el lector a un objeto Solicitud
                            return MapearSolicitud(reader);
                        }
                        return null; // No encontrado
                    }
                }
            }
        }

        public bool Actualizar(Solicitud s)
        {
            // La lógica de UPDATE está aquí. Solo permitimos actualizar campos modificables.
            try
            {
                using (SqlConnection conexion = new SqlConnection(ConnectionString))
                {
                    conexion.Open();

                    // Sentencia SQL: Actualiza campos de contacto y el límite de crédito (SOLO si está APROBADO)
                    string sql = @"
                        UPDATE Solicitudes SET 
                            -- Si s.NombreCliente es vacío, mantén el valor actual de la columna.
                            NombreCliente = CASE WHEN @Nombre = '' THEN NombreCliente ELSE @Nombre END, 
            
                            Direccion = CASE WHEN @Dir = '' THEN Direccion ELSE @Dir END, 
            
                            TelefonoCelular = CASE WHEN @Tel = '' THEN TelefonoCelular ELSE @Tel END, 
            
                            Correo = CASE WHEN @Correo = '' THEN Correo ELSE @Correo END,
            
                            -- Los campos no modificados (Ocupacion y Limite) se dejan sin tocar o se manejan de esta forma:
            
                            Ocupacion = CASE WHEN @Ocupacion = '' THEN Ocupacion ELSE @Ocupacion END, 
            
                            -- La lógica de LimiteCredito debe actualizarse solo si es APROBADO Y si el valor no es cero (placeholder)
                            LimiteCredito = CASE 
                                                WHEN StatusSolicitud = 'Aprobado' AND @Limite > 0 THEN @Limite
                                                ELSE LimiteCredito 
                            END
                        WHERE Folio = @Folio";

                    using (SqlCommand cmd = new SqlCommand(sql, conexion))
                    {
                        // 1. Parámetros de Identificación y Actualización
                        cmd.Parameters.Add("@Folio", SqlDbType.VarChar, 50).Value = s.Folio;
                        cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = s.NombreCliente;
                        cmd.Parameters.Add("@Dir", SqlDbType.VarChar, 255).Value = s.Direccion;
                        cmd.Parameters.Add("@Tel", SqlDbType.VarChar, 20).Value = s.TelefonoCelular;
                        cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value = s.Correo;
                        cmd.Parameters.Add("@Ocupacion", SqlDbType.VarChar, 50).Value = s.Ocupacion;

                        // 2. Parámetro Condicional LimiteCredito
                        // Usa ?? DBNull.Value para manejar si s.LimiteCredito es null (aunque se recomienda enviar un valor)
                        cmd.Parameters.Add("@Limite", SqlDbType.Decimal).Value = s.LimiteCredito ?? 0.00m;

                        // Ejecutar el comando UPDATE
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // En un sistema real, aquí se registraría el error.
                Console.WriteLine($"Error en la BD al actualizar el registro {s.Folio}: {ex.Message}");
                return false;
            }
        }

        public bool Eliminar(string folio)
        {
            // Implementación refactorizada de HandleDelete (DELETE SQL)
            using (SqlConnection conexion = new SqlConnection(ConnectionString))
            {
                conexion.Open();
                string sql = "DELETE FROM Solicitudes WHERE Folio = @Folio";
                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.Add("@Folio", SqlDbType.VarChar, 50).Value = folio;
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}