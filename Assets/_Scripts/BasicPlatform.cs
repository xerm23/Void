using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicPlatform : MonoBehaviour
{
    private Vector3 mainCamPos;

    private void Start()
    {
        this.transform.SetParent(GameObject.Find("GameManager").GetComponent<Transform>());
    }

    private void Update()
    {
        // mainCamPos = Camera.main.transform.position;	
        if (this.gameObject.transform.position.y < Camera.main.transform.position.y - 30 && PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }

}