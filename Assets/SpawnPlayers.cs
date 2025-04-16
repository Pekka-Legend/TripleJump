using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class SpawnPlayers : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PhotonNetwork.Instantiate("Player", new Vector3(-1.38f, 3.17f, -100f), Quaternion.identity);
    }
    public void startGame()
    {
        Hashtable hash = new Hashtable();
        hash.Add("currentPlayer", 0);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

    }

}
