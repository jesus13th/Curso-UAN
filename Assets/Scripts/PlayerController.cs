using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : Character {//heredamos de la clase abstracta Character, la idea era poner la mayor parte de este codigo para poder crear diferentes personajes con habilidades propias sin escribir tanto codigo
    [SerializeField] private Rigidbody _rigidbody;//definimos una variable para el rigidbody
    [SerializeField] private Animator _animator;//definimos una variable para el animator
    private CameraManager _camera;//definimos una variable para la camara

    [SerializeField] private float speedTarget;//definimos una variable para almacenar la velocidad objetivo
    [SerializeField] private float speedRunningTarget;//definimos una variable para almacenar la velocidad objetivo de correr
    [SerializeField] private float speedCurrent;//definimos una variable para almacenar la velocidad actual
    private Vector2 axis;//definimos una variable para almacenar el control del movimiento (teclas o stick)
    private float refSpeed;//definimos una variable para almacenar la velocidad que regresa la interpolacion
    [SerializeField] private float speedSmooth;//definimos una variable para la velocidad de transicion en la interpolacion
    private float angle;//definimos una variable para almacenar el angulo del personaje
    [SerializeField] private float rotationSmooth;//definimos una variable para la velocidad de transicion en la rotacion
    private Quaternion targetRotation;//definimos una variable para almacenar la rotacion objetivo
    [SerializeField] private float jumpForce;//definimos una variable para almacenar la fuerza del salto
    [SerializeField] private LayerMask groundLayer;//definimos una variable para almacenar las capas que seran con las que detecte si esta tocando el piso
    [SerializeField] private float maxDistance;//definimos una variable para almacenar la distancia maxima del suelo
    [SerializeField] private bool IsOnGround;//definimos una variable para almacenar si esta tocando el suelo
    private int extraJump = 1;//definimos una variable para almacenar los saltos que tiene el personaje
    [SerializeField] private bool isRunning;//definimos una variable para decir si esta corriendo o no
    [SerializeField] private int health = 100;//definimos una variable para almacenar la vida del personaje
    [SerializeField] private Slider healthSlider;//definimos una variable para el slider de la salud
    [SerializeField] private TMP_Text healthText;//definimos una variable para el texto de la salud
    [SerializeField] private bool isDead = false;//definimos una variable para almacenar si el personaje esta muerto o no

    [SerializeField] private AudioClip jumpClip;//definimos una variable para almacenar el clip de audio del personaje saltando

    private void Awake() {//se ejecuta al iniciar el juego
        _camera = Camera.main.GetComponent<CameraManager>(); //En base a la camara principal obtenemos el script "CameraManager"
        healthSlider.maxValue = health;//Al slider le asignamos a su valor maximo el valor de la salud(health)
        healthSlider.value = health;//Al slider le asignamos a su valor el valor de la salud(health)
        healthText.text = $"Vida: {health}";//al texto que muestra la salud mostramos el texto $"vida: {health}"-- en c# se puede concatenar un string con el signo de pesos $ y poniendo la variable entre { }
    }

    void Update() {//se ejecuta en cada frame (60 veces por segundo si el juego va a 60FPS)
        IsOnGround = Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, maxDistance, groundLayer);//lanzamos un raycast desde la posicion del personaje y un poco mas arriba,hacia abajo, si intersecta con algo regresa true y sino entonces false

        if (IsOnGround) {//verificamos si el personaje esta sobre el piso
            extraJump = 1;//volvemos a asignar la variable a 1
            axis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));//creamos un vector2 en base al valor que regresa el eje horizontal y el eje vertical
            isRunning = Input.GetKey(KeyCode.LeftShift);//detectamos si se esta presionando la tecla LeftShift y lo asignamos a la variable

            if (Input.GetKeyDown(KeyCode.LeftAlt)) {//detectamos si se presiona la tecla LeftAlt
                _animator.SetTrigger("Dodge");//al animator le activamos el trigger que esta como parametro
            }
        }
        speedCurrent = Mathf.SmoothDamp(speedCurrent, (isRunning ? speedRunningTarget : speedTarget) * axis.magnitude, ref refSpeed, speedSmooth);//hacemos una interpolacion entre el valor actual y el otro valor, que depende de si esta corriendo ademas lo multiplicamos por la magnitud del axis para que sea acorde al control del personaje, el ref solo almacena el cambio que esta teniendo la variable y el smooth es la velocidad en la que cambia. --aqui usamos un "if ternario" este se representa con el signo ? y :, ejemplo, "true ? 1 : 2" si la condicion es verdadera entonces el valor sera 1 y si es falsa entonces 2.

        if (axis.magnitude > 0.1f) {//verificamos que la magnitud del axis sea masyor a 0.1 para rotar al personaje
            angle = Mathf.Atan2(axis.y, -axis.x) * Mathf.Rad2Deg;//calculamos el angulo en base a el axis, usamos la funcion Atan2 que regresa el angulo con dos valor y luego lo convertimos de radianes a grados.
            targetRotation = Quaternion.Euler(0, angle + 180 - _camera.Angle, 0);//asignamos el valor a un quaternion, sumandole en "Y" 180 grados y retandole la rotacion de la camara en "Y".
        }
        if (Input.GetKeyDown(KeyCode.Space) && (IsOnGround || extraJump >= 1)) {//verificamos si se presiona la tecla espacio, y ademas si esta sobre el piso o tiene saltos extras
            extraJump--;//restamos un salto
            _rigidbody.AddForce(Vector3.up * jumpForce * _rigidbody.mass);//agregamos fuerza atraves del rigidbody en direccion hacia arriba y lo multiplicamos por una fuerza y ademas la masa del personaje.
            AudioManager.Instance.PlaySound(jumpClip);//reproducimos un clip atraves del AudioManager
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotationSmooth);//creamos una interpolacion entre la rotacion actual y la rotacion que creamos en la parte de arriba y luego la asignamos al personaje.
    }
    private void FixedUpdate() {//se utiliza para actualizar fisicas
        _rigidbody.velocity = new Vector3(transform.forward.x * speedCurrent, _rigidbody.velocity.y, transform.forward.z * speedCurrent);//apicamos velocidad al rigidbody, la velocidad es el forward del transform (osea hacia delante), pero no modificamos el eje Y para que este pueda caer de forma normal
    }
    private void LateUpdate() {//se utiliza para actualizar animaciones
        _animator.SetFloat("Velocity", !IsOnGround ? 0 : (speedCurrent / (isRunning ? speedRunningTarget : speedTarget)));//al animator le aplicamos un float al valor "Velocity", si el jugador no esta tocando el piso (!IsOnGround) entonces la velocidad sera 0 y si esta tocando, entonces se divide la velocidad actual entre la velocidad objetivo, dependiendo si esta corriendo o no
        _animator.SetBool("IsRunning", isRunning);//al animator le aplicamos un bool para decirle si esta corriendo o no
    }
    private void OnDrawGizmos() {//dibujas formas en la escena
        Gizmos.color = Color.blue;//pintamos los gizmos de color azul
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + Vector3.down * maxDistance);//dibujamos una linea de la posicion del personaje hacia abajo, esto nos ayuda a saber si esta tocando el piso
    }
    public void ApplyDamage(int damage) {//este metodo lo llamamos cuando queremos aplicar un daño
        if (isDead) {//verificamos si el personaje ya esta muerto
            return;//si ya lo esta, entonces detenemos el codigo en esta linea
        }
        health -= damage;//estamos al health el valor de damage

        healthSlider.value = health;//asignamos la nueva salud al slider
        healthText.text = $"Vida: {health}";//asignamos la nueva salud al text

        if (health <= 0) {//verificamos que la salud sea menor o igual a 0
            isDead = true;//si lo es, entonces asignamos "isDead" como true
            _animator.SetTrigger("IsDead");//reproducimos la animacion de dead llamando el trigger atraves del animator
        }
    }
}