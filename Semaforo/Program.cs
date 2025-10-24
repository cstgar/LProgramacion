// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
using System;
using System.Threading;

namespace hilossm
{
    class Program
    {
        static Thread[] threads = new Thread[10];
        static Semaphore sem = new Semaphore(3, 3);

        static void MetodoSemaforo()
        {
            //Metodo sincronizado por semaforo
            Console.WriteLine("{0} esta esperando en la cola...", Thread.CurrentThread.Name);
            sem.WaitOne(); //Establce semaforo para que se detengan los hilos y esperen en la cola
            Console.WriteLine("{0} entrando a metodo con semaforo", Thread.CurrentThread.Name);
            Thread.Sleep(300); //Simula la ejecucion de un proceso que tardaria 300 milisegundos pausa
            Console.WriteLine("{0} saliendo de metodo con semaforo", Thread.CurrentThread.Name);
            sem.Release(); //Libera semaforo una vez terminada la ejecucion del hilo
        }

        static void Main(string[] args)
        {
            //Iniciar hilos a sicnronizar con semaforo
            for (int i = 0; i < 10; i++)
            {
                threads[i] = new Thread(MetodoSemaforo); //Crear hilo y pasar metodo a ejecutar como parametro
                threads[i].Name = "hilo " + i; //Nombrar hilo para identificarlo en este caso hilo mas el numero de iteracion
                threads[i].Start(); //Iniciar ejecucion de hilo

            }
            Console.ReadKey();
        }
    }
}