#include <stdio.h>
#include <winsock2.h>
#include <string.h>
#define PORT 6505

int main()
{
    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2,2), &wsaData) != 0) 
    {
        printf("Error al iniciar Winsock\n");
        return -1;
    }

    int sock = 0;
    struct sockaddr_in serv_addr;
    const char *hello = "Mensaje de texto del cliente";

    if ((sock = socket(AF_INET, SOCK_STREAM, 0)) < 0)
    {
        printf("Error al crear el socket\n");
        return -1;
    }

    serv_addr.sin_family = AF_INET;
    serv_addr.sin_port = htons(PORT);
    serv_addr.sin_addr.s_addr = inet_addr("25.0.248.40"); // IP del servidor

    if (serv_addr.sin_addr.s_addr == INADDR_NONE) 
    {
        printf("Dirección IP no válida\n");
        return -1;
    }

    if (connect(sock, (struct sockaddr *)&serv_addr, sizeof(serv_addr)) < 0)
    {
        printf("Fallo de conexión\n");
        return -1;
    }

    send(sock, hello, strlen(hello), 0);
    printf("Mensaje enviado\n");

    WSACleanup();
    return 0;
}
