using System;
using UnityEngine;
using UnityEngine.UI;

namespace Characters.Scripts
{
    public class CharacterStats : MonoBehaviour
    {
        private void Update()
        {
            if (Camera.current)
            {
                transform.LookAt(Camera.current.transform.position);
            }
        }
    }
}
