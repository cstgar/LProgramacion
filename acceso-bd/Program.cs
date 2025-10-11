// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
using System;
using Microsoft.Data.SqlClient;  //Acceder a una base de datos del tipo Aql Server
using System.Data; //ADO .Net

namespace bdterminal
{
    class Program 
    {
        static void Main(string[] args) 
        {
            int opcion = 0;
            SqlConnection conexion;
            Console.WriteLine("ABC Productos");
            Console.WriteLine("Opciones: 1.- Insertar elemento a tabla. 2.- Consultar elemento.");
            opcion = Convert.ToInt32(Console.ReadLine());
            conexion = new SqlConnection("Data Source=localhost; Database=lp_abc_multi; User Id=access_cinthia; Password=Salome123; TrustServerCertificate=True;");  //Crea el objeto que manejara la base de datos
            conexion.Open(); //Abre la conexion

            switch (opcion)
            {
                case 1:  // opcion Insertar
                {
                    //Sentenciua sql a ejecutar
                    //clase SqlCommand permite crear sentencias sql y construirlas en tiempo de ejecucion, en este caso parametrizarlas
                    SqlCommand cmdConsultar = new SqlCommand("Insert into productos(clave, descripcion) values (@Clave, @Descripcion)", conexion);
                    // Agregar parametros
                    Console.WriteLine("Clave");
                    cmdConsultar.Parameters.Add("@Clave", SqlDbType.VarChar, 10).Value = Console.ReadLine();
                    Console.WriteLine("Descripcion");
                    cmdConsultar.Parameters.Add("@Descripcion", SqlDbType.VarChar, 50).Value = Console.ReadLine();
                    //Ejecutar sentencia sql contra base de datos
                    cmdConsultar.ExecuteNonQuery();
                    Console.WriteLine("Registro inserdado");
                    break;
                }  

                case 2: //opcion Buscar
                {
                        //Sentencia sql a ejecutar
                        Console.WriteLine("Clave a buscar");
                        SqlCommand cmdConsultar = new SqlCommand("select * from productos where clave='" + Console.ReadLine() + "'", conexion);
                        //Ejecuta la sentencia sql y maneja los resultados derivados de una consulta SELECT hecha con sqlcommand, como una tabvla en memoria
                        SqlDataReader reader = cmdConsultar.ExecuteReader();
                        //Comprobar si hay datos en el datareader, si es que se cumplio la consulta sql
                        if (reader.Read())
                        {
                            //Si se encontraron datos, entonces mostrar la descripcion del producto
                            Console.WriteLine($"--- Producto Encontrado ---");
                            Console.WriteLine($"Clave: {reader["clave"].ToString()}");
                            Console.WriteLine($"Descripción: {reader["descripcion"].ToString()}");
                            Console.WriteLine($"--------------------------");
                        }
                        else
                        { 
                            Console.WriteLine("No hay datos que coincidan con la busqueda");
                        }
                        reader.Close(); // Cerrar el data reader
                        break;
                }
            }
            conexion.Close(); // cerrar la conexion
            //Para mantener la ventana y saber si se ejecuto correctamente
            Console.WriteLine("Presiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}