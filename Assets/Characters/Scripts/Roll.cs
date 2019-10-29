using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roll : StateMachineBehaviour {

    public int rollspeed = 10;
    private Vector3 moveDirection;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        if (moveDirection.magnitude == 0)
        {
            moveDirection = animator.transform.forward;
        }
        moveDirection *= rollspeed;
        Debug.Log(moveDirection);
    }

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        moveDirection *= .98f;
        animator.transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        animator.GetComponent<CharacterController>().Move((moveDirection + (Vector3.down * 8f)) * Time.deltaTime);
    }
}
