using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Walls : MonoBehaviour
{
    Transform room;
    Transform currentHit;

    // Start is called before the first frame update
    void Start()
    {
        DOTween.SetTweensCapacity(5000, 50);
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(transform.position + Vector3.up, Vector3.down * 5);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            if (hit.transform != currentHit)
            {
                currentHit = hit.transform;
                Transform currentRoom = getRoom(hit.transform);
                if (!currentRoom || currentRoom != room)
                {
                    if (currentRoom)
                        currentRoom.GetComponent<Room>().Tween(.2f);
                    if (room)
                        room.GetComponent<Room>().Tween(1f);
                    room = currentRoom;
                }

            }
        }
    }

    private Transform getRoom(Transform child)
    {
        if (child.parent == null)
        {
            return null;
        } else if (child.parent.CompareTag("Room"))
        {
            return child.parent;
        } else
        {
            return getRoom(child.parent);
        }
    }
}
