using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;//How to Hashtable (unnecessary for this but I wrote it down incase I forget)
public class LoginManager : MonoBehaviourPunCallbacks
{
    public GameObject[] screens;
    private int index = 0;
    public GameObject startButton;
    public TextMeshProUGUI oppName;
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
        index = 0;
    }
    public void changeName()
    {
        if (screens[index].GetComponentInChildren<TMP_InputField>().text.Length > 0 && screens[index].GetComponentInChildren<TMP_InputField>().text.Length <= 15)
        {
            PhotonNetwork.NickName = screens[index].GetComponentInChildren<TMP_InputField>().text;
            index = 2;
            screens = changeScreen(screens);
        }
            
    }
    public override void OnJoinedLobby()
    {
        index = 1;
        screens = changeScreen(screens);
    }
    public void CreateRoom()
    {
        if (screens[index].GetComponentInChildren<TMP_InputField>().text.Length > 0 && screens[index].GetComponentInChildren<TMP_InputField>().text.Length <= 15)
        {
            int maxPlayers = 8;
            RoomOptions roomOptions = new RoomOptions();

            roomOptions.MaxPlayers = System.Convert.ToByte(maxPlayers + 1);

            PhotonNetwork.CreateRoom(screens[index].GetComponentInChildren<TMP_InputField>().text, roomOptions);
        }
    }
    public void JoinRoom()
    {
        
        PhotonNetwork.JoinRoom(screens[index].GetComponentInChildren<TMP_InputField>().text);
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(1);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        foreach (Player p in PhotonNetwork.PlayerListOthers)
        {
            oppName.text = p.NickName;
        }
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            oppName.text = "Unknown";
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        foreach (Player p in PhotonNetwork.PlayerListOthers)
        {
            oppName.text = p.NickName;
        }
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            oppName.text = "Unknown";
        }
    }
    
    public void startMatch()
    {
        
    }


}
