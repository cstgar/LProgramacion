#include <iostream>

using namespace std;

class FiguraGeometrica { //Superclase
    private:
    string strNombre;

    //Constructor default -> no recibe parametros
    public:
    FiguraGeometrica () {}

    //Constructor con sobrecarga -> inicializa el atributo de la clase con el valor que le llega  como parametro
    FiguraGeometrica(string strN) {
        strNombre = strN;
    }

    //Metodo para calcular el area que sera re-implementado es decir usando POLIMORFISMO. Solo manda el mensaje/
    //En las subclases se le da funcionalidad completa
    double CalcularAreaFigura (double b, double a) {
        cout << "Calculara el area de la figura geometrica: " + strNombre << endl;
        return 0;
    }

};

class Rectangulo : FiguraGeometrica {  //Hereda carcateristicas [ATRIBUTOS] y comportamientos [METODOS] de la superclase
    //Constructor default de subclase. Llama a la version sobrecargada del constructor de la superclase que recibe un parametro
    public:
    Rectangulo () : FiguraGeometrica ("RECTANGULO") {}

    //Reimplementa el metodo CalcularAreaFigura agregando la funcionalidad para que calcule el area, ademas de la que ya tiene [POLIMORFISMO]
    double CalcularAreaFigura (double b, double a) {
        FiguraGeometrica ::CalcularAreaFigura (b, a); //Se llama a la funcionalidad que ya se tiene
        return a*b; // Especificamente se agrega el calculo para el rectangulo
    }
};

class Triangulo : FiguraGeometrica {  //Hereda carcateristicas [ATRIBUTOS] y comportamientos [METODOS] de la superclase
    //Constructor default de subclase. Llama a la version sobrecargada del constructor de la superclase que recibe un parametro
    public:
    Triangulo () : FiguraGeometrica ("TRIANGULO") {}

    //Reimplementa el metodo CalcularAreaFigura agregando la funcionalidad para que calcule el area, ademas de la que ya tiene [POLIMORFISMO]
    double CalcularAreaFigura (double b, double a) {
        FiguraGeometrica ::CalcularAreaFigura (b, a); //Se llama a la funcionalidad que ya se tiene
        return a*b/2; // Especificamente se agrega el calculo para el rectangulo
    }
};

int main () {
    //Declaracion de variables
     int opc = 0;
     double alt, bas, res;

     //Creacion de objetos a utilizar
     Rectangulo rect;
     Triangulo tria;

     //Impresion de consola
     cout << "Demostracion de elementos de programacion orientada a objetos en C++" << endl;

     //Ciclo while
     while (opc != 3) {
        cout << "Calculo del area de diferentes figuras. 1: Rectangulo. 2: Triangulo. 3: Salir." << endl;
        cin >> opc;

        //Condicional switch
        switch (opc)
        {
            case 1: {
                cout << "Proporciona el valor de la base: " << endl;
                cin >> bas;
                cout << "Proporciona el valor de la altura: " << endl;
                cin >> alt;
                res = rect.CalcularAreaFigura(bas, alt); //Llamar al metodo para calcular area del rectangulo
                cout << "El are del rectangulo es: " + to_string(res) << endl;
                break;
            }
            case 2: {
                cout << "Proporciona el valor de la base: " << endl;
                cin >> bas;
                cout << "Proporciona el valor de la altura: " << endl;
                cin >> alt;
                res = tria.CalcularAreaFigura(bas, alt); //Llamar al metodo para calcular area del rectangulo
                cout << "El are del triangulo es: " + to_string(res) << endl;
                break;
            }

           
        }
     }

}