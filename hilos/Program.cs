// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hilos1
{ 
    class Program
    {
        static Thread t1;
        static Thread t2;

        public static void MetodoHilo1()
        {
            //Metodo que se ejecuta con el hilo 1
            Console.WriteLine("Inciando hilo 1 \n");
            for (int x = 0; x < 10; x++)
            {
                Console.WriteLine("Imprimiendo en pantalla hilo 1   vuelta numero ---->" + x + "\n");
            }
            Console.WriteLine("Terminando hilo 1 \n");
        }

        public static void MetodoHilo2()
        {
            //Metodo que se ejecutara con hilo 2
            Console.WriteLine("Inciando hilo 2 \n");
            for (int x = 0; x < 10; x++)
            {
                Console.WriteLine("Imprimiendo en pantalla hilo 2   vuelta numero ---->" + x + "\n");
            }
            Console.WriteLine("Terminando hilo 2 \n");
        }

        static void Main(string[] args)
        {
            //Crea hilos secundarios
            t1 = new Thread(MetodoHilo1); //hilo secundario 1 que se le pasa como parametro el metodo a ejecutar
            t1.Start();
            t2 = new Thread(MetodoHilo2);
            t2.Start();
            Console.WriteLine("Iniciando hilo principal \n"); //Holo Principal que tambien se le llama hilo main
            for (int x = 0; x < 19; x++)
            {
                Console.WriteLine("Imprimiendo en pantalla hilo principal numero " + x);
            }

            Console.WriteLine("Terminando en pantalla el hilo MAIN" + "\n");
            Console.ReadKey();
        }
    }
}