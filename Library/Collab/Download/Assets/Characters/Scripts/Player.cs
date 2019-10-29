using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using UnityEngine;

namespace Characters.Scripts
{
    public class Player : Humanoid
    {
        public static Player Instance;
        public static List<GameObject> Lockables = new List<GameObject>();

        private void Awake()
        {
            Instance = this;
            Lockables.AddRange(GameObject.FindGameObjectsWithTag("Lockable"));
        }

        protected override void SetInputs()
        {
            if (Camera.main != null)
            {
                var camRotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);
                CharInput.MoveVector = camRotation * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
                CharInput.Blocking = Input.GetButton("Aiming");

                var nearestPos = Vector3.zero;
                foreach (var obj in Lockables)
                {
                    if (obj == null)
                    {
                        Lockables.Remove(obj);
                        return;
                    }
                    var pos = Camera.main.WorldToScreenPoint(obj.transform.position);
                    var dir = pos - Input.mousePosition;
                    if (dir.magnitude < 100)
                    {
                        if (nearestPos == Vector3.zero || dir.magnitude < (nearestPos - Input.mousePosition).magnitude)
                        {
                            nearestPos = obj.transform.position;
                        }
                    }
                }

                if (nearestPos == Vector3.zero)
                {
                    if (Camera.main != null && CharInput.Blocking)
                    {
                        CharInput.Aiming = true;
                        CharInput.LookVector = camRotation * (Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)).ToVector3();
                    }
                    else
                    {
                        CharInput.Aiming = false;
                    }
                }
                else
                {
                    CharInput.Aiming = true;
                    CharInput.LookVector = (nearestPos - transform.position);
                }
            }

            CharInput.Attack = Input.GetButtonDown("Attack");
            CharInput.Roll = Input.GetButtonDown("Roll");

            CurrentSpeed = Mathf.Lerp(CurrentSpeed, Input.GetButton("Sprinting") ? runSpeed : walkSpeed, Time.deltaTime * 10);
            CharInput.Sprinting = Input.GetButton("Sprinting");
        }
    }
}
