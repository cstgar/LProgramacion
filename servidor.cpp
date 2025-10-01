#include <unistd.h>
#include <stdio.h>
#include <winsock2.h> // Contiene sys/socket.h y netinet/in.h de Windows
#include <ws2tcpip.h> // Para algunas funciones modernas como getaddrinfo
#include <stdlib.h>
#include <string.h>
#define PORT 6505

int main(int argc, char const *argv[])
{

    // INICIALIZACIÓN DE WINSOCK (NECESARIO)
    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
        fprintf(stderr, "WSAStartup falló.\n");
        return 1;
    }
                
    int server_fd, new_socket, valread;
    struct sockaddr_in address;
    int opt = 1;
    int addrlen = sizeof(address);
    char buffer[1024] = {0};

    //Crear socket
    if ((server_fd = socket(AF_INET, SOCK_STREAM, 0)) == 0)
    {
        perror("Error al crear el socket");
        exit(EXIT_FAILURE);
    }
    if (setsockopt(server_fd, SOL_SOCKET, SO_REUSEADDR, (const char*)&opt, sizeof(opt)))
    {
        perror("Error al crear el socket");
        exit(EXIT_FAILURE);
    }
    address.sin_family = AF_INET;
    address.sin_addr.s_addr = inet_addr("25.0.248.40");
    address.sin_port= htons(PORT);
    if (bind(server_fd, (struct sockaddr *)&address, sizeof(address)) <0)  //Asociar IP y puerto  se hace cast explicito 
    {
        perror("Error al sociar socket a IP y puerto");
        exit(EXIT_FAILURE);
    }
    if (listen(server_fd, 3) < 0) //Poner socket en escucha para conexiones
    {
        perror("Error al poner el socket en escucha");
        exit(EXIT_FAILURE);
    }
    if ((new_socket = accept(server_fd, (struct sockaddr *)&address, (socklen_t*)&addrlen)) < 0)
    {
        perror("Error al aceptar la conexion");
        exit(EXIT_FAILURE);
    }
    valread = read(new_socket, buffer, 1024);
    printf("%s\n", buffer);

    // LIMPIEZA DE WINSOCK (NECESARIO)
    WSACleanup();
    return 0;
}

