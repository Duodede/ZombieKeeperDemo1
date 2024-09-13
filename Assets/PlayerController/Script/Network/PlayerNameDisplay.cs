using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEditor.Rendering;

public class PlayerNameDisplay : MonoBehaviourPunCallbacks
{
    public Text nickName;
    void Start()
    {
        if(photonView.IsMine)
        {
            nickName.text = "";
        }
        else
        {
            nickName.text = photonView.Owner.NickName;
        }
    }
    private void Update()
    {
        nickName.transform.LookAt(2*(nickName.transform.position-Camera.main.transform.position)+ Camera.main.transform.position);
    }
}
