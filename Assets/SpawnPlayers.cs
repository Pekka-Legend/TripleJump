using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class SpawnPlayers : MonoBehaviour
{
    public GameObject startButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PhotonNetwork.Instantiate("Player", new Vector3(-1.38f, 3.17f, -100f), Quaternion.identity);
        if (!PhotonNetwork.IsMasterClient) startButton.SetActive(false);
        
    }
    public void startGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int currentPlayer = 1;
            Hashtable roomProps = new Hashtable();
            roomProps["currentPlayer"] = currentPlayer;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
        startButton.SetActive(false);

    }
    private void Update()
    {
        
    }

}
