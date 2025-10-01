import socket #Importando el modulo estandar de Python con acceso a la funcionalidad de sockets

HOST = '25.0.248.40' #Definiendo la IP de host es decir a la que se va a conectar de tipo servidor
PORT = 65432

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s :  #Creando socket basado en flujos
    s.connect((HOST, PORT))  #Conectandose a traves de la IP y Puerto para enviar informacion
    s.sendall(b'Mensaje de texto del cliente')  #Enviando informacion  al servidor. La letra b indica que es un objeto de bytes en vez de u
print('Mensaje Enviado!!!')