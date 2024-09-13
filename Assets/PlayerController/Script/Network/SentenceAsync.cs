using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
public class SentenceAsync : MonoBehaviourPunCallbacks, IPunObservable
{
    public string str;
    public GameObject pa;

    private void Start()
    { 
        pa = GameObject.FindWithTag("Content");
        this.transform.SetParent(pa.transform);
    }
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(str);
        }
        else
        {
            str = (string)stream.ReceiveNext();
            this.GetComponent<Text>().text = str;
        }
    }
}
