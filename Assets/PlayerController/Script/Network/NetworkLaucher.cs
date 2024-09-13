using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;

public class NetworkLaucher : MonoBehaviourPunCallbacks
{
    public InputField nameInput;
    public InputField roomNameInput;
    public Text joinButtonText;
    public Button joinButton;
    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        joinButtonText.text = "�ȴ���������";
        joinButton.interactable = false;
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("�ɹ����ӵ�������");
        joinButtonText.text = "��������뷿��";
        joinButton.interactable = true;
    }
    public void JoinOrCreateARoom()
    {
        PhotonNetwork.NickName = nameInput.text;
        if (roomNameInput.text == null)
            return;
        RoomOptions options = new RoomOptions() { MaxPlayers = 4};
        PhotonNetwork.JoinOrCreateRoom(roomNameInput.text, options, default);
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(1);
    }
}
