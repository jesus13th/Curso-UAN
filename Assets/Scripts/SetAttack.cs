using UnityEngine;

public class SetAttack : StateMachineBehaviour//Clase especial para las animaciones, estas sirve para ejecutar codigo al iniciar, cuando se actualiza o cuando termina una animacion
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<ZombieController>().IsAttacking = true;//obtenemos del animator la clase ZombieController y a su valirable IsAttacking la asignamos como true
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<ZombieController>().IsAttacking = false;//obtenemos del animator la clase ZombieController y a su valirable IsAttacking la asignamos como false
    }
}
