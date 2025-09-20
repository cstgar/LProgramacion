 ;; Prueba
 (* 1 2 3)
;;Ejemplo 1
(defun calcular-suma (a b)
        (+ a b))

 (calcular-suma 89 13)
; Descubri que esto es un comentario con punto y coma
;
; Y hasta que detecte una funcionalidad regresa la terminal con el simbolo de asterisco (*)
;Ejemplo 2
(defun es-mayor (x y)
        (if (> x y)
                (format t "~a es mayor que ~a" x y)
                (format t "~a no esmayor que ~a" x y)
        )
)

;;en Lisp el condicional if tiene 2 opciones la condicion y la alternativa es decir no tiene un else explicito como otros lenguajes
;;otra curiosidad es que la t significa true es decir defines las sentencias verdaderas sin embargo no existe false
;;nil significa falso y tambien representa el valor vacio
;;format es el print en otros lenguejes
;;~a es el formato mas comun sin embargo hay otros por ejemplo ~% que es salto de linea
;;ahora procedemos a llamar la funcion
(es-mayor 34 17)

;; Ejemplo 3
(defun factorial (num)
        (if (<= num 1)
                1
                (* num (factorial (- num 1))
                )
        )
)

;; factorial --> es el producto de todos los numeros enteros positivos desde 1 hata n
(factorial 4)