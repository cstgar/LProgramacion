# Uso de procedimientos
def MostrarIstrucciones():
    print("Demostracion de uso de condiciones, ciclos, procedimientos y funciones en Python")

# Uso de funciones
def CalcularAreaRectangulo (b, a):
    return (b * a)
def CalcularAreaTriangulo (b, a):
    return (b*a)/2

opc = ""
b= 0.0
a= 0.0
r= 0.0

MostrarIstrucciones()

while opc != "s":
    print("Calculo de area de figuras. r: Rectangulo. t: Triangulo. s: Salir.")
    opc = input()

    if opc == "r":
        print("Proporciona el valor de la base: ")
        b = float(input()) # Esto es la conversion implicita de tipos de datos
        print("Proporciona el valor de la altura: ")
        a = float(input())
        r = CalcularAreaRectangulo(b, a) # Esta es la llamada a la funcion del rectangulo
        print("El area del rectangulo es: " + str(r))
    elif opc == "t":
        print("Proporciona el valor de la base: ")
        b = float(input()) # Conversion implicita de tipos de datos
        print("Proporciona el valor de la altura: ")
        a = float(input())
        r = CalcularAreaTriangulo(b, a) # Llamada a la funicion del triangulo
        print("El area del triangulo es: " + str(r))
