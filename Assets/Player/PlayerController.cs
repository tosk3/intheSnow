using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]private int health;
    [SerializeField]private float movementMaxTime = 0.5f;
    [SerializeField]private float movementTimer = 0f;
    Vector3 newPosition = new Vector3();

    void Start()
    {
        
    }

    void Update()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (movementTimer <= 0f)
        {


            if (Input.GetAxis("Horizontal") >= 0.01f && Input.GetAxis("Horizontal") < 2)
            {
                newPosition.x += 1f;
                transform.position = newPosition;
                movementTimer = movementMaxTime;

            }
            else if (Input.GetAxis("Horizontal") <= -0.01f && Input.GetAxis("Horizontal") > -2)
            {
                newPosition.x -= 1f;
                transform.position = newPosition;
                movementTimer = movementMaxTime;
            }
            else if (Input.GetAxis("Vertical") >= 0.01f && Input.GetAxisRaw("Vertical") < 2)
            {
                newPosition.y += 1f;
                transform.position = newPosition;
                movementTimer = movementMaxTime;
            }
            else if (Input.GetAxis("Vertical") <= -0.01f && Input.GetAxis("Vertical") > -2)
            {
                newPosition.y -= 1f;
                transform.position = newPosition;
                movementTimer = movementMaxTime;
            }
            
        }
        else
        {
            movementTimer -= Time.deltaTime;
        }
       
    }
}
