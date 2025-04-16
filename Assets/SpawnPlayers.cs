using UnityEngine;
using Photon.Pun;
public class SpawnPlayers : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PhotonNetwork.Instantiate("Player", new Vector3(-1.38f, 3.17f, -100f), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
