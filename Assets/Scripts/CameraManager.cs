using UnityEngine;

public class CameraManager : MonoBehaviour {
    private Transform tTarget;//definimos el objetivo de la camara
    private Vector3 refVelocity;//creamos variable para almacenar el delta de la posicion en la interpolacion
    [SerializeField] private float smooth;//creamos un smooth para indicar la velocidad de la transicion de la posicion
    [SerializeField] private float smoothRotation;//creamos un smooth para indicar la velocidad de la transicion de la rotacion

    [SerializeField] private float angle = 270;//definimos un angulo
    public float Angle {//creamos una propiedad llamada angle
        private set => angle = value;//al set lo hacemos privado, yle decimos que cuando se asigne, se le asigne a su otra variable. esta solo la podremos asignar desde esta clase.
        get => angle;// al get es publico, osea lo podemos obtener desde otra clase. la fecha => es una expresion lambda y sirve para reducir codigo o crear funciones anonimas. esto es igual que "get { return angle }"
    }

    [SerializeField] private float distance = 2;//definimos una variable para la distancia que tendra la camara del personaje
    [SerializeField] private float height = 2;//definimos una varibale para la altura que tendra la camara del personaje
    void Start() {
        tTarget = GameObject.FindWithTag("Player").transform;//buscamos el objeto con la etiquta "Player", obtenemos su transform y se lo asignamos al objetivo de la camara
    }

    void Update() {
        Angle -= Input.GetAxis("Mouse X");//obtenemos el delta del mouse en el eje X y se lo restamos al angulo
        distance -= Input.mouseScrollDelta.y / 10f;//obtenemos el valor del scroll del mouse, lo dividimos entre 10 y se lo restamos a la distancia
        distance = Mathf.Clamp(distance, 1, 4);//limitamos el valor de distancia entre dos numeros con la funcion Clamp, el valor minimo que puede tener es 1 y el maximo es 4
        Vector3 pos = new Vector3(Mathf.Cos(Angle * Mathf.Deg2Rad), 0, Mathf.Sin(Angle * Mathf.Deg2Rad));//definimos un vector que tendra la posicion a la que seguira la camara, en base al angulo usamos la funcion Cos y Sin para sacar el X y Y
        Vector3 targetPosition = tTarget.position + pos * distance + Vector3.up * height;//creamos una variable para almacenar la posicion final, considerando la posicion del pesonaje, la posicion en base al angulo, la distancia y la altura
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref refVelocity, smooth * Time.deltaTime);//asignamos la posicion y hacemos una interpolacion de la posicion actual y la posicion objetivo

        Vector3 dir = (tTarget.position - transform.position).normalized;//calculamos la direcion entre el personaje y la camara y luego la normalizamos
        Quaternion targetRotation = Quaternion.Euler(25, Mathf.Atan2(-dir.z, dir.x) * Mathf.Rad2Deg + 90, 0);//definimos una variable para almacenar la rotacion objetivo, esta seria en su eje Y el angulo de la direccion utilizando la funcion Atan2 y pasandole los parametros de la direccion, luego lo pasamos de radianes a grados y le sumamos 90 grados. 
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * smoothRotation);//creamos una interpolacion de la rotacion actual y la rotacion objetivo
    }
}