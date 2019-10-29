using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterExitBool : StateMachineBehaviour {

    public string boolName;
    public bool enter = true;
    public bool exit;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetBool(boolName, enter);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetBool(boolName, exit);
    }
}
