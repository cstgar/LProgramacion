#include <winsock2.h>
#include <ws2tcpip.h>
#include <stdio.h>
#include <stdlib.h>

#define PORT 6505

int main() {
    WSADATA wsaData;
    SOCKET server_fd, new_socket;
    struct sockaddr_in address, client_addr;
    int addrlen = sizeof(address);
    char buffer[1024] = {0};
    int opt = 1;

    // Inicializar Winsock
    if (WSAStartup(MAKEWORD(2,2), &wsaData) != 0) {
        printf("WSAStartup falló.\n");
        return 1;
    }

    // Crear socket
    if ((server_fd = socket(AF_INET, SOCK_STREAM, 0)) == INVALID_SOCKET) {
        printf("Error al crear socket\n");
        WSACleanup();
        return 1;
    }

    // Reusar puerto
    setsockopt(server_fd, SOL_SOCKET, SO_REUSEADDR, (const char*)&opt, sizeof(opt));

    // Dirección
    address.sin_family = AF_INET;
    address.sin_addr.s_addr = INADDR_ANY;   // Escuchar en todas las interfaces
    address.sin_port = htons(PORT);

    // Asociar socket a IP/puerto
    if (bind(server_fd, (struct sockaddr *)&address, sizeof(address)) == SOCKET_ERROR) {
        printf("Error en bind: %d\n", WSAGetLastError());
        closesocket(server_fd);
        WSACleanup();
        return 1;
    }

    // Escuchar conexiones
    if (listen(server_fd, 3) == SOCKET_ERROR) {
        printf("Error en listen: %d\n", WSAGetLastError());
        closesocket(server_fd);
        WSACleanup();
        return 1;
    }

    printf("Servidor en espera en el puerto %d...\n", PORT);

    // Aceptar cliente
    int client_size = sizeof(client_addr);
    new_socket = accept(server_fd, (struct sockaddr *)&client_addr, &client_size);
    if (new_socket == INVALID_SOCKET) {
        printf("Error en accept: %d\n", WSAGetLastError());
        closesocket(server_fd);
        WSACleanup();
        return 1;
    }

    // Confirmación de conexión
    printf("Cliente conectado desde %s:%d\n",
           inet_ntoa(client_addr.sin_addr),
           ntohs(client_addr.sin_port));

    // Recibir mensaje del cliente
    int valread = recv(new_socket, buffer, sizeof(buffer) - 1, 0);
    if (valread > 0) {
        buffer[valread] = '\0'; // Terminar cadena
        printf("Mensaje recibido: %s\n", buffer);
    } else {
        printf("No se recibió ningún mensaje o conexión cerrada. Código: %d\n", WSAGetLastError());
    }

    // Cerrar sockets
    closesocket(new_socket);
    closesocket(server_fd);
    WSACleanup();

    return 0;
}
