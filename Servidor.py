import socket  #Importando el modulo estandar de Python con acceso a laa funcionalidad de socket

HOST = '25.0.248.40'  #Direccion IP Servidor
PORT = 65432  #Puerto a abrir como Servidor

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.bind((HOST, PORT))  #Asociar IP y Puertos
    print('Servidor escuchando en {}:{}'.format(HOST, PORT))  #Se agrega para saber que si esta en escucha
    s.listen()  #Poner en escucha el servidor de socket para conexiones
    conn, addr = s.accept()   #Acepta la peticion de conexion (normalmente antes de hacerlo se valida que el cliente es confiable
    with conn:
        print('Conectado a {}'.format(addr))  #Mostrando datos del cliente conectado
        while True:
            data = conn.recv(1024)   #Mientras haya datos en el bufer recibirlos
            if not data:
                break
            print('Recibido', repr(data))