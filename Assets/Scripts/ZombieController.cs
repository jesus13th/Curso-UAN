using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

public enum ZombieState { None, Patrol, Chase, Search, Attack }//creamos un enum para tener diferentes estados del zombie
public class ZombieController : MonoBehaviour {
    [SerializeField] private PlayerController target;//definimos una variable para almacenar el target del zombie
    [SerializeField] private List<Transform> nodes;//creamos una lista de Transform los cuales seran los nodos del camino que recorrera el zombie
    [SerializeField] private NavMeshAgent navMesh;//definimos una variable para el navMesh
    [SerializeField] private Animator _animator;//definimos una variable para el animator

    private bool flagAttack;//creamos un flag para el ataque
    private bool isAttacking;//creamos una variable para saber si esta atacando
    public bool IsAttacking {//creamos una propiedad para saber si esta atacando
        get => isAttacking;//al obtenerlo devolvemos la variable de isAttacking
        set {//al asignarlo
            isAttacking = value;//asignamos el valor a la variable de isAttacking
            if (!value) {//si es valor es falso
                flagAttack = true;//poenoms el flag como true
            }
        }
    }
    private int currentNode;//definimos una variable para el nodo actual
    private int CurrentNode {//definimos una propiedad para el nodo actual
        get { return currentNode; }//al obtener el valor regresamos la variable del nodo actual
        set {//al asignar el valor
            currentNode = value;//asignamos el valor a la variable del nodo actual
            currentNode = currentNode % nodes.Count;//usamos el operador mod (%) para evitar que pase de la longitud de los nodos, ejemplo, si la longitud es 10 y el valor es 11, entonces el operador nos regresara ese 1 restante. El operador mod (%) es el operador que devuelve el residuo de una division
            navMesh.SetDestination(nodes[currentNode].position);//al navmesh le asignamos la posicion del nodo actual
        }
    }
    private float distance;//definimos una variable para almacenar la distancia entre el zombie y el jugador
    [SerializeField] private float rangeDistance;//definimos una variable para poner un rango de distancia
    private float angle;//definimos una variable para almacenar el angulo entre el zombie y el jugador
    [SerializeField] private float rangeAngle;//definimos una variable para poner un rango del angulo
    private bool canSee = false;//definimos una variable para almacenar si el zombie puede ver al jugador
    private float timer;//definimos una variable para almacenar un contador de tiempo

    //
    private bool canAttack;//definimos una variable para decir si puede atacar el zombie
    [SerializeField] private float attackRangeDistance;//definimos una variable para tener un rango de distancia en el cual puede atacar
    [SerializeField] private float attackRangeAngle;//definimos una variable para para tener un rango del angulo en el cual puede atacar
    [SerializeField] private float delayChasing;//definimos una variable para almacenar el tiempo de espera que debe de tener antes de dejar de perseguir
    [SerializeField] private float delaySearching;//definimos una variable para almacenar el tiempo de espera que debe de tener antes de dejar de buscar
    [SerializeField] private Transform hand;//definimos una variable para el transform de la mano del zombie
    [SerializeField] private LayerMask characterLayerMask;//definimos una variable para almacenar los layermask con los cuales puede interactuar el ataque del zombie
    [SerializeField] private Vector3 handSize;//definimos una variable para establecer el tamaño que tendra el hitbox de la mano del zombie

    [SerializeField] private ZombieState state;//creamos una variable de la enumeracion para almacenar el stado actual
    public ZombieState State {//definimos una propiedad para el estado del zombie
        private set {//al asignarlo
            if (state != value) {//verificamos que el nuevo valor sea diferente al actual
                state = value;//si es diferente entonces lo asignamos, esto evita que se ejecute el mismo codigo cuando no es necesario
                switch (state) {//hacemos un switch evaluando el stado
                    case ZombieState.Patrol://si el estado es Patrol
                        CancelInvoke(nameof(ChaseTarget));//dejamos de perseguir al objetivo
                        navMesh.isStopped = false;//hacemos que el navmesh se vuelva a mover
                        CurrentNode = CurrentNode;//reactivamos al patrullaje llamando la propiedad CurrentNode, le asignamos su mismo valor para que regrese al nodo en el que se quedo
                        break;
                    case ZombieState.Chase://si el estado es Chase
                        InvokeRepeating(nameof(ChaseTarget), 0, 0.5f);//llamamos el metodo "chaseTarget repetidamente con el metodo InvokeRepeating, el primer parametro es el nombre del metodo, el segundo es el tiempo que tardara en ejecutarse por primera vez y el tercero es el tiempo que tardara en estar ejecutando el metodo
                        navMesh.isStopped = false;//hacemos que el navmesh se vuelva a mover
                        break;
                    case ZombieState.Search://si el estado es Search
                        CancelInvoke(nameof(ChaseTarget));//dejamos de perseguir al objetivo
                        navMesh.isStopped = true;//hacemos que el navmesh se detenga
                        break;
                    case ZombieState.Attack://si el estado es Attack
                        StartCoroutine(AttackCoroutine());//iniciamos una corrutina para atacar al personaje
                        break;
                }
            }
        }
        get => state;//al obtenerlo devolvemos el stado del zombie
    }
    void Start() {
        State = ZombieState.Patrol;//asignamos a la propiedad State el estado de Patrol, esto hara que comience a patrullar
    }
    private void Update() {
        CalculateMeasures();//llamamos al metodo que calcula la distancia, el angulo y si lo puede ver

        Collider[] colliders = Physics.OverlapBox(hand.position, handSize, hand.rotation, characterLayerMask);//definimos un array de collider y le asignamos lo que regresa la funcion Overlap, que basicamente crea un hitbox, su primer parametro es la posicion, el segundo el tamaño, el tercero la rotacion y el cuarto el layermask con el que puede interactuar
        if (flagAttack && isAttacking && colliders.Length > 0) {//verificamos que el zombie pueda atacar y que ademas este collisionando con algo
            colliders[0].GetComponent<PlayerController>().ApplyDamage(10);//obtenemos el primer elemento del array, obtenemos el PlayerController y llamamos a su metodo ApplyDamage y le decimos que el daño sera de 10
            flagAttack = false;//asignamos el flag de attack como false
        }

        if (canAttack) {//si el personaje puede atacar entonces
            State = ZombieState.Attack;//cambiamos al estado de Attack
            return;//detenemos el codigo aqui
        }

        if (State == ZombieState.Patrol || State == ZombieState.Search) {//si el estado del zombie es patrol o search
            if (canSee) {//y si lo puede ver
                State = ZombieState.Chase;//entonces persigue al personaje
                timer = 0;//iniciamos el contado en 0
            } else if (!canSee && State != ZombieState.Patrol) {//si ya no lo puede ver y su estado es diferente a patrol
                timer += Time.deltaTime;//comenzamos a contar, sumamos al timer el delta time. el deltaTime es la diferencia entre un frame y otro, basicamente si un juego va a 60FPS, entonces el deltaTime seria 1/60, esto equivale 0.016666, esta cantidad se sumara cada frame, entonces luego de 60 frames, se sumara 1 en 60 frames
                if (timer > delaySearching) {//si el conado es mayor al delay de searching entonces es porque no lo encontro
                    State = ZombieState.Patrol;//regresamos al estado de patrullar
                    timer = 0;//asignamos el contador a 0
                }
            }
        } else if (State == ZombieState.Chase) {//si el estado es igual a chase
            if (!canSee) {//el zombie no puede ver al personaje
                timer += Time.deltaTime;//comienza a contar
                if (timer > delayChasing) {//si el contador es mayor al timepo de perseguir, entonces no lo alcanzo
                    State = ZombieState.Search;//pasa al estado de buscar
                    timer = 0;//asignamos el contador a 0
                }
            } else {//si el zombie puede ver al personaje
                timer = 0;//asignamos el contador a 0
            }
        }
    }
    private void LateUpdate() {
        float velocity = navMesh.velocity.magnitude / navMesh.desiredVelocity.magnitude;//calculamos la velocidad actual en base al navmesh y lo dividimos entre su velocidad deseada, usamos las magnitudes de ambos para obtener un numero en lugar de un vector
        _animator.SetFloat("Velocity", float.IsNaN(velocity) ? 0 : velocity);//al animator le asignamos el valor al parametro velocity, primero verificamos que velocity si no es un numero, entonces le asignamos 0 y si lo es entonces asignamos el valor de velocity
    }
    private void OnTriggerEnter(Collider other) {//cuando entra on contacto con un trigger, el parametro es el objeto con el que entro en contacto
        if (other.CompareTag("Node") && other.transform == nodes[currentNode] && State == ZombieState.Patrol) {//si el objeto tiene la etiqueta "Node", ademas es el nodo que sigue y el estado es Patrol
            CurrentNode++;//entones avanzamos al siguiente nodo
        }
    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;//ponemos el color rojo a los gizmos

        //dibujamos una linea desde la posicion del personaje hacia diferentes direcciones
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, rangeAngle, 0) * transform.forward * rangeDistance));//hacia la direccion del frente con rangeAngle de rotacion y lo multiplicamos por el rangeDistance
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, rangeAngle / 2.0f, 0) * transform.forward * rangeDistance));//hacia la direccion del frente con rangeAngle entre 2 de rotacion y lo multiplicamos por el rangeDistance
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, 0, 0) * transform.forward * rangeDistance));//hacia la direccion del frente con 0 grados de rotacion y lo multiplicamos por el rangeDistance
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, -rangeAngle / 2.0f, 0) * transform.forward * rangeDistance));//hacia la direccion del frente con rangeAngle negativo entre 2 de rotacion y lo multiplicamos por el rangeDistance
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, -rangeAngle, 0) * transform.forward * rangeDistance));//hacia la direccion del frente con rangeAngle negativo de rotacion y lo multiplicamos por el rangeDistance

        Gizmos.color = Color.blue;//ponemos el color azul a los gizmos
        for (int i = 0; i < nodes.Count; i++) {//recorremos los nodos
            Gizmos.DrawLine(nodes[i].position, nodes[(i + 1) % nodes.Count].position);//dibujamos una linea entre el nodo actual y el siguiente, y hacemos un Mod para que cuando llegue al ultimo, entonces dibuje la linea hacia el primero
        }

        Gizmos.color = State switch {//dependiendo del estado del zombie es el color que sera el gizmo
            ZombieState.None => Color.black,
            ZombieState.Patrol => Color.green,
            ZombieState.Chase => Color.yellow,
            ZombieState.Search => Color.blue,
            ZombieState.Attack => Color.red,
            _ => throw new System.NotImplementedException()
        };
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, 0.1f);//dibujamos una esfera sobre el zombie

        Gizmos.color = Color.red;//ponemos el color rojo a los gizmos
        Gizmos.DrawWireCube(hand.position, handSize);//dibujamos un cubo en la mano del zombie
    }
    private void CalculateMeasures() {//calculamos ciertas medidas
        Vector3 direction = (target.transform.position - transform.position).normalized;//obtenemos la direccion entre el objetivo y el zombie y luego lo normalizamos
        direction.y = 0;//asignamos su valor de Y en 0
        angle = Vector3.Angle(direction, transform.forward);//calculamos el angulo con la funcion Angle de la clase Quaternion, donde paramos la variable direcion y la direcion que queremos, en este caso es forward
        distance = Vector3.Distance(target.transform.position, transform.position);//calculamos la distancia con la funcion Distance de la clase Vector, le pasamos el parametro de la posicion del target y del zombie
        canSee = distance < rangeDistance && angle < rangeAngle;//si la distancia es menor al rango de distancia y el angulo es menor al rango del angulo, entonces el valor se lo asignamos a canSee
        canAttack = distance < attackRangeDistance && angle < attackRangeAngle;//si la distancia y el angulo son menores a sus rangos de ataque, entonces le asignamos ese valor a canAttack
    }
    private IEnumerator AttackCoroutine() {//Creamos una corrutina para el ataque
        CancelInvoke(nameof(ChaseTarget));//cancelamos el invoke que hace que persiga al personaje
        navMesh.isStopped = true;//detenemos al navmesh
        _animator.SetTrigger("Attack");//en el animator llamamos el triggues de "Attack"
        yield return new WaitForSeconds(1);//Esperamos un segundo
        State = ZombieState.Chase;//Cambiamos el estado a perseguir
    }
    private void ChaseTarget() => navMesh.SetDestination(target.transform.position);//creamos un metodo que le asigna la posicion del personaje al navmesh
}