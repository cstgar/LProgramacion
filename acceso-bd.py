import pyodbc

# Declarar la variable de conexion con la base de datos y crear objeto para manejar la conexion
connection = pyodbc.connect(
    'DRIVER={ODBC Driver 17 for SQL Server}; SERVER=localhost; PORT=1443; DATABASE=lp_abc_multi; UID=access_cinthia; PWD=Salome123')

print("ABC Productos")
print("Opciones disponibles. 1.-Insertar elemento. 2.-Buscar elemento.")
opcion = input()
if opcion == "1":  # Insertar
    print("Clave")
    clave = input()
    print("Descripcion")
    descripcion = input()
    cursorProductos = connection.cursor()

    # Sentencia sql a ejecutar
    sql = "INSERT INTO productos (clave, descripcion) VALUES (?, ?)"
    # Agregar parametros
    val = (clave, descripcion)

    # Ejecutar sentencia sql contra base de datos
    cursorProductos.execute(sql, val)
    connection.commit()
elif opcion == "2":
    print("Clave")
    clave = input()
    cursorProductos = connection.cursor()

    # Sentencia sql a ejecutar para select
    sql = "SELECT * FROM productos WHERE clave = ?"
    clv = (clave,)
    cursorProductos.execute(sql, clv)

    # Si hubo datos entonces se va mostrar la descripcion del producto
    myResult = cursorProductos.fetchall()
    for row in myResult:
        print(row[1])