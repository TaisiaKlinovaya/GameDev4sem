using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class MiniCat : NetworkBehaviour
{
    private int clickCount = 0;

    private void OnMouseDown()
    {
        if (!IsOwner) return;

        ClickServerRpc();
    }

    [ServerRpc]
    private void ClickServerRpc()
    {
        clickCount++;
        if (clickCount >= 3)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
