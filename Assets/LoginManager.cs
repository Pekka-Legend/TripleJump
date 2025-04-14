using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
public class LoginManager : MonoBehaviourPunCallbacks
{
    public GameObject[] screens;
    private int index = 0;
    public GameObject startButton;
    public GameObject oppName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        screens = changeScreen(screens);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private GameObject[] changeScreen(GameObject[] screens)
    {
        for (int i = 0; i < screens.Length; i++)
        {
            if (i != index)
                screens[i].SetActive(false);
            else
                screens[i].SetActive(true);
        }
        return screens;
    }
    public override void OnConnectedToMaster()
    {
        
        PhotonNetwork.JoinLobby();
    }
    public void changeName()
    {
        PhotonNetwork.LocalPlayer.NickName = screens[index].GetComponentInChildren<TMP_InputField>().text;
        index++;
        screens = changeScreen(screens);
    }
    public override void OnJoinedLobby()
    {
        index++;
        screens = changeScreen(screens);
    }
    public void toRoomJoin()
    {
        index++;
        screens = changeScreen(screens);
    }
    public void CreateRoom()
    {
        int maxPlayers = 2;
        RoomOptions roomOptions = new RoomOptions();

        roomOptions.MaxPlayers = System.Convert.ToByte(maxPlayers + 1);

        PhotonNetwork.CreateRoom(screens[index].GetComponentInChildren<TMP_InputField>().text, roomOptions);
    }
    public void JoinRoom()
    {
        
        PhotonNetwork.JoinRoom(screens[index].GetComponentInChildren<TMP_InputField>().text);
    }
    public override void OnJoinedRoom()
    {
        index++;
        screens = changeScreen(screens);
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            startButton.SetActive(true);
    }
    public void startMatch()
    {
        
    }
    
}
