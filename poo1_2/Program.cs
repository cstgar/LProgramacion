using System;

namespace poo1_2
{
    class FiguraGeometrica //Es la superclase compuesta por 2 constructores:
    {
        private string strNombre;

        //Constructor default que no recibe parametros
        public FiguraGeometrica()
        {
        }

        //Consutructor con sobrecarga - Inicializa un atributo de la clase con el valor que le llega como parametro
        public FiguraGeometrica(string strN)
        {
            strNombre = strN;
        }

        //Metodo para calcular el area de las figuras que sera re-implementado [Polimorfismo].
        //Solo manda el mensaje y ya en las subclases se implementa la funcionalidad completa.
        //Es decir su unico objetico es re implementarlo en las subclases de la superclase FiguraGeometrica
        public virtual double CalcularAreaFigura(double bas, double altura)
        { 
            Console.WriteLine("Se calculara el area de la figura geometrica" + strNombre);
            return 0;
        }
    }

    class Rectangulo : FiguraGeometrica   // clase rectangulo hereda caracteristicas [atributos] y comportamientos [metodos] de la superclase
    {
        //Constructor default de subclase. Llama a la version sobrecargada del constructor de la superclase que recibe un parametro
        public Rectangulo() : base("Rectangulo") { }

        //Reimplementa el metodo calcular area figura agregandole la funcinolidad para que calcule el area, ademas de la que ya tiene (polimorfismo)
        public override double CalcularAreaFigura(double bas, double altura)
        {
            base.CalcularAreaFigura(bas, altura);  //Aqui se llama a la funcionalidad que ya tiene
            return bas * altura; //Especificamente se agrega la funcionalidad para el rectangulo
        }
    }

    class Triangulo : FiguraGeometrica   // clase triangulo hereda caracteristicas [atributos] y comportamientos [metodos] de la superclase
    {
        //Constructor default de subclase. Llama a la version sobrecargada del constructor de la superclase que recibe un parametro
        public Triangulo() : base("Triangulo") { }

        //Reimplementa el metodo calcular area figura agregandole la funcinolidad para que calcule el area, ademas de la que ya tiene (polimorfismo)
        public override double CalcularAreaFigura(double bas, double altura)
        {
            base.CalcularAreaFigura(bas, altura);  //Aqui se llama a la funcionalidad que ya tiene
            return bas * altura / 2; //Especificamente se agrega la funcionalidad para el triangulo
        }
    }

    class poo1_2
    {
        //metodo main
        static void Main(string[] args)
        {
            //Declaracion de variables
            string opc = "";
            double bas, alt, res;

            //Creacion de objetos a utilizar mediante la palabra reservada new
            Rectangulo rectangulo = new Rectangulo();
            Triangulo triangulo = new Triangulo();

            //Impresion en consola
            Console.WriteLine("Demostracion de practica para los elementos que forman parte de programacion orientada a objetos en C#");

            //Ciclo while
            while (! opc.Equals("s")) //Condicion mietras que opc NO sea igual a "s"
            {
                Console.WriteLine("Calculo del area de diferentes figuras. r: Rectangulo. t: Triangulo. s: Salir");
                opc = Console.ReadLine()!;

                //Condicional switch
                switch (opc)
                {
                    case "r":
                        {
                            //Impresion de pantalla
                            Console.WriteLine("Proporciona el valor de la base: ");
                            bas = Convert.ToDouble(Console.ReadLine());
                            Console.WriteLine("Proporciona el valor de la altura: ");
                            alt = Convert.ToDouble(Console.ReadLine());
                            res = rectangulo.CalcularAreaFigura(bas, alt); //Llamada al metodo CalcularAreaFigura de la subclase Rectangulo
                            Console.WriteLine("El area del RECTANGULO es: " + res.ToString());
                            break;
                        }
                    case "t":
                        {
                            //Impresion de pantalla
                            Console.WriteLine("Proporciona el valor de la base: ");
                            bas = Convert.ToDouble(Console.ReadLine());
                            Console.WriteLine("Proporciona el valor de la altura: ");
                            alt = Convert.ToDouble(Console.ReadLine());
                            res = triangulo.CalcularAreaFigura(bas, alt); //Llamada al metodo CalcularAreaFigura de la subclase Rectangulo
                            Console.WriteLine("El area del TRIANGULO es: " + res.ToString());
                            break;
                        }

                }
            }
            Console.WriteLine("Saliendo...");

            //Aqui en ms visual studio le agrego la espera de un enter para poder revisar los resultados hasta cuando sale del ciclo while
            Console.ReadLine();
        }
    }

}