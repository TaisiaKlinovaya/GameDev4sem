using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : NetworkBehaviour
{
    Vector3 inputMovement;
    public float speed = 2f;
    private Vector3 otherPos;


    void Start()
    {
        inputMovement = Vector3.zero;
    }

    void Update()
    {
        if (IsOwner)
        {
            inputMovement.x = Input.GetAxis("Horizontal");
            inputMovement.y = Input.GetAxis("Vertical");

            transform.position += inputMovement * Time.deltaTime * speed;
            if (NetworkManager.Singleton.IsClient)
            {
                MoveServerRpc(transform.position);
            }
        }
        else
        {
            transform.position = otherPos;
        }
    }
    [ServerRpc]
    void MoveServerRpc(Vector3 pos)
    {
        otherPos = pos;
        MoveClientRpc(pos);
    }

    [ClientRpc]
    void MoveClientRpc(Vector3 pos)
    {
        otherPos = pos;
    }


}
