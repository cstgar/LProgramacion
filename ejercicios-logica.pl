/*%Ejercicio 1 usando prolog
%Definiendo Hehos
animal(perro).
animal(gato).
animal(pato).

tiene_patas(perro, 4).
tiene_patas(gato, 4).
tiene_patas(pato, 2).

%Definiendo Reglas
es_mamifero(X) :- animal(X), tiene_patas(X, 4).
*/

% Ejercicio 2 usando prolog: Encontrar la ruta entre 2 ciudades

% Definir hechos de rutas directas
conectado(la_paz, todos_santos).
conectado(todos_santos, cabo_san_lucas).
conectado(cabo_san_lucas, los_cabos).

%Definir la regla de ruta directa o indirecta
ruta(X,Y) :- conectado(X,Y).    %ruta directa
ruta(X,Y) :- conectado(X,Z), ruta(Z,Y).     %ruta indirecta
