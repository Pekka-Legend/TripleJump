using UnityEngine;
using Photon.Pun;
public class LoginManager : MonoBehaviourPunCallbacks
{
    public GameObject[] screens;
    private int index;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
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
        index++;
        screens = changeScreen(screens);
    }
    
}
