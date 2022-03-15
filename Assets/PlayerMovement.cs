using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  [SerializeField] float moveSpeed = 0.25f;

    Vector3 targetPosition;
    Vector3 startPosition;
    bool moving;

    void Update()
    {
        if (moving)
        {
            if (Vector3.Distance(startPosition, transform.position) > 1f)
            {
                transform.position = targetPosition;
                moving = false;
                return;
            }

            transform.position += (targetPosition - startPosition) * moveSpeed * Time.deltaTime;
            return;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            targetPosition = transform.position + Vector3.forward;
            startPosition = transform.position;
            moving = true;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            targetPosition = transform.position + Vector3.back;
            startPosition = transform.position;
            moving = true;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            targetPosition = transform.position + Vector3.left;
            startPosition = transform.position;
            moving = true;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            targetPosition = transform.position + Vector3.right;
            startPosition = transform.position;
            moving = true;
        }
    }
}
