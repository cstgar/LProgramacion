#include <iostream>

using namespace std;

// Uso de procedimientos
void MostrarInstrucciones ()
{
    //Impresion en consola
    cout << "Demostracion de uso de condiciones, ciclos, procedimientos y funciones en C++" << endl;
}

// Uso de funciones
double CalcularAreaRectangulo (double b, double a)
{
    return b * a;
}

double CalcularAreaTriangulo (double b, double a)
{
    return b*a/2;
}

int main()    //punto de entrada es el metodo main
{
    //Opciones de menu
    int opc = 0;
    double b, a, r;

    MostrarInstrucciones();

    //Ciclo while
    while (opc != 3)
    {
        cout << "Calculo del area de diferentes figuras. 1: Rectangulo, 2:Triangulo, 3: Salir" << endl;
        cin >> opc;

        //Condicional switch
        switch (opc)
        {
            case 1: {
                cout << "Proporciona el valor de la base: " << endl;
                cin >> b;
                cout << "Proporciona el valor de la altura: " << endl;
                cin >> a;
                r = CalcularAreaRectangulo(b, a);   //  Llamada a la funcion para rectangulo
                cout <<"El area del rectangulo es: " + to_string(r) << endl;
                break;
            }
            case 2: {
                cout << "Proporciona el valor de la base: " << endl;
                cin >> b;
                cout << "Proporciona el valor de la altura: " << endl;
                cin >> a;
                r = CalcularAreaTriangulo(b, a);   //  Llamada a la funcion para triangulo
                cout <<"El area del triangulo es: " + to_string(r) << endl;
                break;
            }
        }
    }
    cout << "Saliendo..." << endl;
    return 0;
}
