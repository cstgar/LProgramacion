// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

using hilosseccioncritica;
using System;
using System.Threading.Tasks;

namespace hilosseccioncritica
{

    public class Cuenta
    {

        private readonly object bloqueoBalance = new object();
        private decimal balance;

        public Cuenta(decimal balanceInicial)
        {
            //Establecer balance inicial en cuenta
            balance = balanceInicial;

        }


        public decimal Retirar(decimal monto)
        {
            //Rertirar de la cuenta con el bloque solo se permite un hilo a la vez
            lock (bloqueoBalance)
            {
                if (balance >= monto)
                {
                    Console.WriteLine($"Balance antes de retiro : {balance,5}");
                    Console.WriteLine($"Cantidad a retirar      : {monto,5}");
                    balance = balance - monto;
                    Console.WriteLine($"Balance después de retiro: {balance,5}");
                    return monto;
                }
                else
                {
                    return 0;
                }
            }

        }

        public void Depositar(decimal monto)
        {
            //Depositar a la cuenta con el bloqueo igual que Retirar solamente se permite un hilo a la vez
            lock (bloqueoBalance)
            {
                Console.WriteLine($"Balance antes de deposito: {balance,5}");
                Console.WriteLine($"Cantidad a depositar:      {monto,5}");
                balance = balance + monto;
                Console.WriteLine($"Balance despues de deposito: {balance,5}");
            }
        }

    }
}

class PruebaCuenta
{
    static void Main()
    {
        //Se crean 100 hilos o tasks para actualizar la cuenta a aleatoriamente se puede depositar o retirar
        var cuenta = new Cuenta(1000);
        var tareas = new Task[100];
        for (int i = 0; i < tareas.Length; i++)
        {
            tareas[i] = Task.Run(() => ActualizarAleatoriamente(cuenta));
        }
        Task.WaitAll(tareas);
        Console.ReadKey();

    }

    static void ActualizarAleatoriamente(Cuenta account)
    {
        //Determin aleatoriamente si se va a depositar o retirar de la cuenta
        var random = new Random();
        for (int i = 0; i < 10; i++)
        {
            var monto = random.Next(1, 100);
            bool bDepositar = random.NextDouble() < 0.5;
            if (bDepositar)
            {
                account.Depositar(monto);
            }
            else
            {
                account.Retirar(monto);
            }

        }
    }
}

