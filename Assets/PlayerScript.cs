using System;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using JetBrains.Annotations;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviourPunCallbacks
{
    public Sprite[] sprites;
    private float timer = 0;
    public float swapTime = 0;
    private int index = 0;
    public GameObject bg;
    private float fullTimer = 0;//full swap time is .45 seconds
    private float step2Time = 0;
    private float pendulumBaseTime = 0;
    private float maxTime = 0;
    private float steps = 0;
    public float speed = 0;
    private bool shouldStart = false;
    private float ySpeed = 0;
    public float jumpForce = 0;
    private float inputDir = -1;
    private bool canRecieveInput = true;
    public float gravity = 9.8f;
    public float gMultiplier = 0;
    private float yVel = 0;
    public float speedDamper;
    private float speedBoost = 0;
    private int jumps = 0;
    private bool shouldEnd = false;
    public Text boostText;
    private PhotonView pv;
    public Camera cam;
    private GameObject startButton;
    private float leaveTime = 0;
    private int shouldChangeActivePlayer = 0;
    private bool hasChangedIndex = false;
    private float showScoreTime = 0;
    private int jumpCount = 0;//this is the number of triple jumps taken, not number of jumps in one triple jump
    public GameObject showScoreObject;
    private int character = 0;
    private GameObject pendulum;
    private int cycles = 0;
    private GameObject scaleL;
    private GameObject scaleR;
    private GameObject scaleM;
    private GameObject[] clouds;
    private String boostTextText = "";
    private bool hasWarped = false;
    private Vector2 flickStart = Vector2.zero;
    private Vector2 flickEnd = Vector2.one;
    private GameObject arrow;
    private int arrowDir = 0;
    public float flickBonus = 0;
    private int streak = 0;
    private GameObject surge;
    public GameObject spotlight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        pendulumBaseTime = swapTime;
        boostText = GameObject.FindGameObjectWithTag("boost").GetComponent<Text>();
        
        pv = GetComponent<PhotonView>();
        if (pv.IsMine)
        {
            
            bg = GameObject.FindGameObjectWithTag("tbg");
            showScoreObject = GameObject.FindGameObjectWithTag("sbg");
            pendulum = GameObject.FindGameObjectWithTag("pen");
            arrow = GameObject.FindGameObjectWithTag("arrow");
            scaleL = GameObject.FindGameObjectWithTag("sl");
            scaleR = GameObject.FindGameObjectWithTag("sr");
            scaleM = GameObject.FindGameObjectWithTag("sm");
            clouds = GameObject.FindGameObjectsWithTag("cloud");
            surge = GameObject.FindGameObjectWithTag("surge");
            
            System.Array.Sort(clouds, (a,b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));
            scaleR.SetActive(false);
            scaleL.SetActive(false);
            scaleM.SetActive(false);
            pendulum.SetActive(false);
            arrow.SetActive(false);
            bg.SetActive(false);
            surge.SetActive(false);
            for (int i = 0; i < clouds.Length; i++)
            {
                clouds[i].gameObject.SetActive(false);
            }
            Hashtable hash = new Hashtable();
            hash.Add("bestJump", 0.0f);

            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            showScoreObject.SetActive(false);
        }
        if (PhotonNetwork.IsMasterClient && !PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("currentPlayer"))
        {
            Hashtable props = new Hashtable();
            props.Add("currentPlayer", 1);
            props.Add("showScore", 0);
            props.Add("jumpCount", 0);
            props.Add("leader", 0);
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
        character = (int)PhotonNetwork.LocalPlayer.CustomProperties["sprite"];


    }

    // Update is called once per frame
    void Update()//-1.38f->22.96f
    {
        
        if (pv.IsMine)
        {
            //if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("leader")) Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties["leader"]);
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("showScore") && (int)PhotonNetwork.CurrentRoom.CustomProperties["showScore"] == 1)
            {
                pv.RPC("showSpotlight", RpcTarget.All, false);
                leaveTime = 0;
                showScoreObject.SetActive(true);
                bg.SetActive(false);
                pv.RPC("changeHUD", RpcTarget.All, 0, " ");
                int i = 0;
                float bestJump = 0;
                String bestJumper = "";
                int lastPlayer = (int)PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"] - 2;
                if (lastPlayer == -1) lastPlayer = PhotonNetwork.CurrentRoom.PlayerCount - 1;
                if ((int)PhotonNetwork.CurrentRoom.CustomProperties["jumpCount"] < 5)
                {
                    foreach (Player p in PhotonNetwork.PlayerList)
                    {
                        showScoreObject.GetComponent<TextComponent>().texts[i].text = p.NickName + ": " + p.CustomProperties["bestJump"];
                        if (lastPlayer == i)//player count is 1-6, i is 0-5
                        {
                            showScoreObject.GetComponent<TextComponent>().texts[6].text = "Last Jump:";
                            showScoreObject.GetComponent<TextComponent>().texts[7].text = p.NickName + ": " + PhotonNetwork.CurrentRoom.CustomProperties["lastJump"];
                        }
                        
                        i++;
                    }

                }
                else
                {
                    foreach (Player p in PhotonNetwork.PlayerList)
                    {
                        if ((float)p.CustomProperties["bestJump"] > bestJump)
                        {
                            bestJump = (float)p.CustomProperties["bestJump"];
                            bestJumper = p.NickName;
                        }
                    }
                    showScoreObject.GetComponent<TextComponent>().texts[0].text = "Winner";
                    showScoreObject.GetComponent<TextComponent>().texts[1].text = bestJumper + ": " + bestJump;
                }
                showScoreTime += Time.deltaTime;
                if (showScoreTime > 3)
                {
                    Hashtable hash = new Hashtable();
                    hash.Add("showScore", 0);
                    float bj = 0;
                    int bji = 0;
                    int index = 1;//start at 1 bc actornumber starts at 1
                    foreach (Player p in PhotonNetwork.PlayerList)
                    {
                        if ((float)p.CustomProperties["bestJump"] > bj)
                        {
                            bj = (float)p.CustomProperties["bestJump"];
                            bji = index;
                        }
                        index++;
                    }
                    hash.Add("leader", bji);
                    PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                    
                    showScoreObject.SetActive(false);
                    if ((int)PhotonNetwork.CurrentRoom.CustomProperties["jumpCount"] == 5)
                    {
                        
                        PhotonNetwork.Disconnect();
                        SceneManager.LoadScene(0);
                    }
                }
                return;
            }
            if (shouldStart && !shouldEnd)
            {
                if ((int)PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"] == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    
                    
                    if (steps >= 14)
                    {
                        if (!hasWarped)
                        {
                            Mouse.current.WarpCursorPosition(new Vector2(480f, Input.mousePosition.y));
                            hasWarped = true;
                            Time.timeScale = .75f;
                        }
                        
                        float pos = Mathf.Clamp(Input.mousePosition.x, 100f, 860f);
                        bg.SetActive(false);
                        pendulum.SetActive(false);
                        if (jumps == 1)
                        {
                            scaleL.SetActive(true);
                            scaleR.SetActive(true);
                            scaleM.SetActive(true);
                            scaleL.transform.localPosition = new Vector2(scaleL.transform.localPosition.x, (((pos - 100f) / 760f) * 30f + 42.5f));
                            scaleR.transform.localPosition = new Vector2(scaleR.transform.localPosition.x, ((-(pos - 100f) / 760f) * 30f + 72.5f));
                        }
                        else
                        {
                            scaleL.SetActive(false);
                            scaleR.SetActive(false);
                            scaleM.SetActive(false);
                            
                        }

                        
                        
                        transform.position = new Vector2(transform.position.x + (speed + speedBoost) * Time.deltaTime, transform.position.y + ySpeed * Time.deltaTime);

                        if (transform.position.y > 4.17f)
                        {
                            index = 3;
                            pv.RPC("changeSprite", RpcTarget.All, index + (5 * character));
                            if (jumps == 1)//if doing the first jump
                            {
                                if (pos > 480)
                                {
                                    speedBoost += (pos - 480) / 1000000f;
                                    gravity = 50;
                                    Time.timeScale = .75f;
                                    for (int i = 0; i < 3; i++) clouds[i].SetActive(false);
                                }
                                else if (pos < 480)
                                {
                                    int cloudsActive = 0;
                                    for (int i = 0; i < 3; i++)
                                    {
                                        if (gMultiplier * (480 - pos) > (3.8f / 3) * i)
                                        {
                                            clouds[i].SetActive(true);
                                            cloudsActive++;
                                        }
                                        else
                                        {
                                            clouds[i].SetActive(false);
                                        }
                                    }
                                    gravity = 50 - (gMultiplier * (480 - pos));
                                    speedBoost -= (480 - pos) / (cloudsActive * 500000f / 3);
                                    Time.timeScale = .75f - (cloudsActive / 6f);
                                }
                            }
                            else if (jumps == 2)
                            {
                                arrow.SetActive(true);
                                arrow.transform.rotation = Quaternion.Euler(new Vector3(arrow.transform.eulerAngles.x, arrow.transform.eulerAngles.y, arrowDir * 90f));
                                Time.timeScale = .35f;
                                if (Input.GetMouseButtonDown(0))
                                {
                                    flickStart = Input.mousePosition;
                                }
                                if (Input.GetMouseButtonUp(0))
                                {
                                    flickEnd = Input.mousePosition;
                                    if (Math.Abs(flickStart.x - flickEnd.x) > Math.Abs(flickStart.y - flickEnd.y))//if the flick is on the x axis (right or left)
                                    {
                                        if (flickStart.x < flickEnd.x)//right
                                        {
                                            Debug.Log("right");
                                            if (arrowDir == 0)
                                            {
                                                streak++;
                                                speedBoost += flickBonus * streak;
                                            }
                                            else
                                            {
                                                speedBoost -= 2 * flickBonus;
                                                streak = 0;
                                            }
                                        }
                                        else//left (technically also middle but idc)
                                        {
                                            Debug.Log("left");
                                            if (arrowDir == 2)
                                            {
                                                streak++;
                                                speedBoost += flickBonus * streak;
                                            }
                                            else
                                            {
                                                speedBoost -= 2 * flickBonus;
                                                streak = 0;
                                            }
                                        }
                                    }
                                    else//flick is up or down
                                    {
                                        if (flickStart.y < flickEnd.y)//up
                                        {
                                            Debug.Log("up");
                                            if (arrowDir == 1)
                                            {
                                                streak++;
                                                speedBoost += flickBonus * streak;
                                            }
                                            else
                                            {
                                                speedBoost -= 2 * flickBonus;
                                                streak = 0;
                                            }
                                        }
                                        else//down (technically also middle but idc)
                                        {
                                            Debug.Log("down");
                                            if (arrowDir == 3)
                                            {
                                                streak++;
                                                speedBoost += flickBonus * streak;
                                            }
                                            else
                                            {
                                                speedBoost -= 2 * flickBonus;
                                                streak = 0;
                                            }
                                        }
                                    }
                                    int pd = arrowDir;
                                    while (pd == arrowDir) arrowDir = Random.Range(0, 4);//can't do the same direction twice in a row
                                }
                            }
                            else if (jumps == 3)
                            {
                                arrow.SetActive(false);
                                surge.SetActive(true);
                                if (Input.GetMouseButtonDown(0))
                                {
                                    speedBoost += .05f;
                                }
                            }

                        }
                        else if (transform.position.y > 3.18f && yVel > 0)//about to land thing
                        {
                            index = 2;
                            pv.RPC("changeSprite", RpcTarget.All, index + (5 * character));

                            arrow.SetActive(false);

                        }
                        else if (transform.position.y > 3.17f)
                        {
                            index = 0;
                            pv.RPC("changeSprite", RpcTarget.All, index + (5 * character));
                            canRecieveInput = true;

                        }
                        else
                        {
                            transform.position = new Vector2(transform.position.x, 3.17f);
                            yVel = jumpForce;
                            ySpeed = 0;
                            jumps++;
                            fullTimer = 0;
                            
                            if (jumps == 4)//you land after the third jump
                            {
                                shouldEnd = true;
                                transform.position = new Vector2(transform.position.x, 2.92f);
                                Hashtable hash = new Hashtable();
                                hash.Add("lastJump", Mathf.Round(transform.position.x * 100) / 100);

                                PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                                if (Mathf.Round(transform.position.x * 100) / 100 > (float)PhotonNetwork.LocalPlayer.CustomProperties["bestJump"])
                                {
                                    hash = new Hashtable();
                                    hash.Add("bestJump", Mathf.Round(transform.position.x * 100) / 100);

                                    PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

                                    
                                }
                                hasChangedIndex = false;
                                showScoreTime = 0;
                            }
                        }
                        yVel -= gravity * Time.deltaTime;
                        ySpeed += yVel * Time.deltaTime;
                        fullTimer += Time.deltaTime;
                    }
                    else
                    {
                        pendulum.SetActive(true);
                        Vector3 ang = pendulum.transform.eulerAngles;
                        if (cycles % 2 == 0)
                        {
                            ang.z = Mathf.Rad2Deg * Mathf.Asin(Mathf.Clamp((-step2Time / (pendulumBaseTime * 6)) * 2 + 1, -1f, 1f));
                        }
                        else
                        {
                            ang.z = Mathf.Rad2Deg * Mathf.Asin(Mathf.Clamp((step2Time / (pendulumBaseTime * 6)) * 2 - 1, -1f, 1f));
                        }
                        //Debug.Log(ang.z);
                        pendulum.transform.rotation = Quaternion.Euler(ang);
                        if (index >= 0 && canRecieveInput)//make some sort of variable called "input was processed" for each step
                        {
                            

                            if (Input.GetMouseButtonDown(0)) inputDir = 0;
                            else inputDir = -1;
                            if (inputDir == 0)
                            {

                                speedBoost += .01f / (Mathf.Abs(step2Time - (pendulumBaseTime * 3)) + .05f);
                                pendulumBaseTime -= (.01f / (Mathf.Abs(step2Time - (pendulumBaseTime * 3)) + .05f)) / 10f;
                                if (pendulumBaseTime < .05f) pendulumBaseTime = .05f;
                                canRecieveInput = false;
                            }



                        }
                        else
                        {
                        }
                        if (timer > swapTime)
                        {
                            index++;

                            if (index > 2)
                            {
                                index = 0;

                                fullTimer = 0;
                                steps++;
                                if (steps % 2 == 0)
                                {


                                }



                            }
                            pv.RPC("changeSprite", RpcTarget.All, index + (5 * character));
                            timer = 0;


                        }
                        if (step2Time > pendulumBaseTime * 6)
                        {
                            step2Time = 0;
                            canRecieveInput = true;
                            cycles++;
                        }
                        timer += Time.deltaTime;
                        fullTimer += Time.deltaTime;
                        step2Time += Time.deltaTime;
                        transform.position = new Vector2(transform.position.x + speed * Time.deltaTime, transform.position.y);
                    }
                    pv.RPC("changeHUD", RpcTarget.All, 0, "Boost: " + (Mathf.RoundToInt(speedBoost * 1000f)).ToString());
                }
                
            }
            else if (!shouldStart)
            {
                if (cam.gameObject.activeInHierarchy && Input.GetKey(KeyCode.Space) && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("currentPlayer") && (int)PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"] == PhotonNetwork.LocalPlayer.ActorNumber) shouldStart = true;
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("currentPlayer") && (int)PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"] == PhotonNetwork.LocalPlayer.ActorNumber && transform.position.x == -1.38f) 
                {
                    arrowDir = Random.Range(0, 4);
                    bg.SetActive(true);
                    pv.RPC("changeHUD", RpcTarget.All, 0, ("Boost: " + Mathf.RoundToInt(speedBoost * 1000f)));
                    if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("leader") && (int)PhotonNetwork.CurrentRoom.CustomProperties["leader"] == PhotonNetwork.LocalPlayer.ActorNumber)
                    {
                        pv.RPC("showSpotlight", RpcTarget.All, true);
                    }
                }
                else
                {
                    bg.SetActive(false);
                    

                }
                if (PhotonNetwork.IsMasterClient)
                {
                    if (PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"] != null)
                    {
                        
                        pv.RPC("SetActiveCamera", RpcTarget.All, (int)PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"]);
                    }
                }
                
                index = 0;
                pv.RPC("changeSprite", RpcTarget.All, index + (5 * character));
                
            }
            else if (shouldEnd)
            {
                index = 4;
                pv.RPC("changeSprite", RpcTarget.All, index + (5 * character));
                surge.SetActive(false);
                if (!hasChangedIndex)
                {
                    Time.timeScale = 1f;
                    for (int i = 0; i < clouds.Length; i++)
                    {
                        clouds[i].gameObject.SetActive(false);
                    }
                    Hashtable hash = new Hashtable();
                    if ((int)PhotonNetwork.CurrentRoom.CustomProperties["showScore"] == 0)
                    {

                        hash.Add("showScore", 1);
                    }
                    if ((int)PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"] + 1 > PhotonNetwork.CurrentRoom.PlayerCount)//when we need to go to the next round
                    {
                        hash.Add("jumpCount", (int)PhotonNetwork.CurrentRoom.CustomProperties["jumpCount"] + 1);
                        hash.Add("currentPlayer", 1);
                        


                    }
                    else
                    {
                        hash.Add("currentPlayer", (int)PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"] + 1);
                        
                    }
                    PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                    hasChangedIndex = true;

                }
                
                if (leaveTime > 3)
                {
                    timer = 0;
                    index = 0;
                    fullTimer = 0;
                    step2Time = 0;
                    steps = 0;
                    shouldStart = false;
                    ySpeed = 0;
                    inputDir = -1;
                    canRecieveInput = true;
                    yVel = 0;
                    speedBoost = 0;
                    jumps = 0;
                    shouldEnd = false;
                    cycles = 0;
                    leaveTime = 0;
                    shouldChangeActivePlayer = 0;
                    pendulumBaseTime = swapTime;
                    showScoreTime = 0;
                    transform.position = new Vector3(-1.38f, 3.17f, -100f);
                    showScoreObject.SetActive(false);
                    Hashtable hash = new Hashtable();
                    Vector3 ang = pendulum.transform.eulerAngles;
                    ang.z = 90;
                    pendulum.transform.rotation = Quaternion.Euler(ang);
                    PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                    scaleL.SetActive(false);
                    scaleR.SetActive(false);
                    scaleM.SetActive(false);
                    hasWarped = false;
                    streak = 0;

                    pv.RPC("masterChangeActivePlayer", RpcTarget.All);
                    
                }
                
                leaveTime += Time.deltaTime;
            }
            if (PhotonNetwork.IsMasterClient && shouldChangeActivePlayer == 1)
            {
                pv.RPC("SetActiveCamera", RpcTarget.All, (int)PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"]);
                shouldChangeActivePlayer = 0;
            }
            if (maxTime < fullTimer)
            {
                maxTime = fullTimer;
            }
        }


    }
    GameObject GetPlayerObject(int actorNumber)
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            PhotonView pv = obj.GetComponent<PhotonView>();
            if (pv != null && pv.Owner.ActorNumber == actorNumber)
            {
                return obj;
            }
        }
        return null;
    }
    [PunRPC]
    public void changeSprite(int index)
    {
        GetComponent<SpriteRenderer>().sprite = sprites[index];
    }
    [PunRPC]
    public void SetActiveCamera(int actorNumber)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PhotonView view = player.GetComponent<PhotonView>();
            Camera cam = player.GetComponentInChildren<Camera>(true);

            if (cam != null)
            {
                bool isActive = view.Owner.ActorNumber == actorNumber;
                cam.gameObject.SetActive(isActive);
            }
        }
    }
    [PunRPC]
    public void changeHUD(int action, String parameter = "")
    {
        Debug.Log(parameter + ", " + action);
        if (action == 0)
        {
            if (boostText != null) boostText.text = parameter;
        }
        else if (action == 1)
        {

        }
    }

    [PunRPC]
    public void masterChangeActivePlayer()
    {
        shouldChangeActivePlayer = 1;
    }
    [PunRPC]
    public void showScore()
    {
        shouldEnd = true;
        shouldStart = true;
        showScoreTime = 0;
    }
    [PunRPC]
    public void showSpotlight(bool setting)
    {
        spotlight.SetActive(setting);
    }
    

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
    }

}
