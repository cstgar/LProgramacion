using System; //es el import en java

namespace codebaselenguaje  // es el package de java
{
    class programacion_estructurada_1_1    //fuertemente orientado a objetos por lo que requiere una clase
    {
        //Uso de procedimientos
        static void MostrarInstrucciones()
        {
            //Impresion en consola
            Console.WriteLine("Demostracion de uso de condiciones, ciclos, procedimientos y funciones en C#");
        }

        //Uso de funciones
        static double CalcularAreaRectangulo(double b, double a)
        {
            return b * a;
        }

        static double CalcularAreaTriangulo(double b, double a)
        {
            return b * a / 2;
        }

        static void Main(string[] args)     //metodo main que es el punto de entrada al programa
        {
            //Declaracion de variables
            string opc = "";
            double a, b, r;

            MostrarInstrucciones();

            //Ciclo while
            while (!opc.Equals("s"))
            {
                Console.WriteLine("Calculo para el area de las siguientes figuras r: Rectangulo. t: Triangulo. s: Salir");
                opc = Console.ReadLine()!;

                //Condicional switch
                switch (opc)
                {
                    case "r":
                        {
                            Console.WriteLine("Proporciona el valor de base:");
                            b = Convert.ToDouble(Console.ReadLine());
                            Console.WriteLine("Proporcionar el valor de la altura");
                            a = Convert.ToDouble(Console.ReadLine());
                            r = CalcularAreaRectangulo(b, a); //Llamada a la funcion
                            Console.WriteLine("El area del rectangulo es: " + r.ToString());
                            break;
                        }
                    case "t":
                        {
                            Console.WriteLine("Proporciona el valor de la base: ");
                            b = Convert.ToDouble(Console.ReadLine());
                            Console.WriteLine("Proporciona el valor de la altura: ");
                            a = Convert.ToDouble(Console.ReadLine());
                            r = CalcularAreaTriangulo(b, a);
                            Console.WriteLine("El area del triangulo es: " + r.ToString());
                            break;
                    }
                }
            }
        }
    }
}