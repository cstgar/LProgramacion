class FiguraGeometrica:
    strNombre = ""

    # Constructor con sobrecarga -> Inicializa un atributo de la clase con el valor que le llega como parametro
    def __init__(self, strN):
        self.strNombre = strN
    # Metodo para calcular el area que sera re-implementada [POLIMORFISMO]. Solo manda el mensaje
    # En las subclases se le da la funcionalidad especifica para cada figura
    def CalcularAreaFigura (self, b, a):
        print("Se calculara el area de la figura geometrica " + self.strNombre)
        return 0

class Rectangulo (FiguraGeometrica):   # Hereda caracteristicas [PROPIEDADES] y comportamientos [METODOS] de la superclase
    # Constructor default de subclase. Llama a la version sobrecargada del constructor de la superclase que recibe un parametro
    def __init__(self):
        FiguraGeometrica.__init__(self, "RECTANGULO")
    # Re-implementa el metodo CalcularAreaFigura agregandole la funcinalidad para que calcule el area del rectangulo, ademas de la que ya tiene [POLIMORFISMO]
    def CalcularAreaFigura (self, b, a):
        super().CalcularAreaFigura(b, a)  # Se llama la funcionalidad que ya tiene
        return (b*a)  # Especificamente se le agrega esta funcionalidad nueva para el rectangulo

class Triangulo (FiguraGeometrica):   # Hereda caracteristicas [PROPIEDADES] y comportamientos [METODOS] de la superclase
    # Constructor default de subclase. Llama a la version sobrecargada del constructor de la superclase que recibe un parametro
    def __init__(self):
        FiguraGeometrica.__init__(self, "TRIANGULO")
    # Re-implementa el metodo CalcularAreaFigura agregandole la funcinalidad para que calcule el area del triangulo, ademas de la que ya tiene [POLIMORFISMO]
    def CalcularAreaFigura (self, b, a):
        super().CalcularAreaFigura(b, a)  # Se llama la funcionalidad que ya tiene
        return (b*a)/2  # Especificamente se le agrega esta funcionalidad nueva para el triangulo

# Declaracion de variables
opc = ""
b = 0.0
a = 0.0
r = 0.0

#Creacion de objetos a utilizar
rect = Rectangulo()
tria = Triangulo()

print("Demostracion de elementos de Programacion Orientada a Objetos en Python")
# Ciclo while
while opc != "s":
    print("Calculo de area de diferentes figuras. r: Rectangulo. t: Triangulo. s: Salir.")
    opc = input()

    # Condicional con if para cada opcion
    if opc == "r":
        print(" *** Seccion Rectangulo ***")
        print("Proporciona el valor de la base: ")
        b = float(input())
        print("Proporciona el valor de la altura: ")
        a = float(input())
        r = rect.CalcularAreaFigura(b, a)  # Esta es la llamada al metodo calcular area del rectangulo
        print("El are del rectangulo es: " + str(r))
    elif opc == "t":
        print(" *** Seccion Triangulo ***")
        print("Proporciona el valor de la base: ")
        b = float(input())
        print("Proporciona el valor de la altura: ")
        a = float(input())
        r = tria.CalcularAreaFigura(b, a)  # Esta es la llamada al metodo calcular area triangulo
        print("El are del triangulo es: " + str(r))
print("Saliendo...")
