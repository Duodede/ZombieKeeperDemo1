using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class NetworkGameManager : MonoBehaviourPunCallbacks
{
    public Transform startPoint;
    public Text testInfo;
    public InputField inputField;
    public Transform content;
    public GameObject typing;
    bool isInstantiated;
    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Start()
    {
        if(PhotonNetwork.CountOfPlayersInRooms == 0)
        {
            PhotonNetwork.Instantiate("Player", startPoint.position, startPoint.rotation, 0);
        }
    }
    public override void OnJoinedRoom() 
    {
        PhotonNetwork.Instantiate("Player", startPoint.position, startPoint.rotation, 0);
    }
    public void Update()
    {
        testInfo.text = "id:" + PhotonNetwork.NickName + " | " + PhotonNetwork.CurrentRoom +" | "+"version:" + PhotonNetwork.AppVersion + " ";
    }
    public void SendMessage()	// ������Ϣ����¼������Ϣ�����ƣ��������͵���Ϣ��Ȼ�󴴽�һ��UI���壬���ص�����content��
    {
        if (inputField.text != "")
        {
            string res = PhotonNetwork.NickName + " : " + inputField.text;
            GameObject obj = PhotonNetwork.Instantiate("DialogText", Vector3.zero, Quaternion.identity);
            obj.GetComponent<Text>().text = res;
            obj.GetComponent<SentenceAsync>().str = res;
            obj.transform.SetParent(content);
            inputField.text = "";	// ���ͺ���������
        }
    }
}
