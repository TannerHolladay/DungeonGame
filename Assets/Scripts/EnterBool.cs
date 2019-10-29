using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterBool : StateMachineBehaviour
{
    public string boolName;
    public bool enter = true;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (boolName == "rootmotion")
        {
            animator.applyRootMotion = enter;
        }
        else
        {
            animator.SetBool(boolName, enter);
        }
    }
}
