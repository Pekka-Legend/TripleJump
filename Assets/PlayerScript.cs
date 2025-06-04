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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boostText = GameObject.FindGameObjectWithTag("boost").GetComponent<Text>();
        pendulumBaseTime = swapTime;
        pv = GetComponent<PhotonView>();
        if (pv.IsMine)
        {
            bg = GameObject.FindGameObjectWithTag("tbg");
            showScoreObject = GameObject.FindGameObjectWithTag("sbg");
            pendulum = GameObject.FindGameObjectWithTag("pen");
            pendulum.SetActive(false);
            bg.SetActive(false);
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
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
        character = (int)PhotonNetwork.LocalPlayer.CustomProperties["sprite"];


    }

    // Update is called once per frame
    void Update()//-1.38f->22.96f
    {
        
        if (pv.IsMine)
        {
            
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("showScore") && (int)PhotonNetwork.CurrentRoom.CustomProperties["showScore"] == 1)
            {
                leaveTime = 0;
                showScoreObject.SetActive(true);
                bg.SetActive(false);
                boostText.text = " ";
                int i = 0;
                float bestJump = 0;
                String bestJumper = "";
                if ((int)PhotonNetwork.CurrentRoom.CustomProperties["jumpCount"] < 5)
                {
                    foreach (Player p in PhotonNetwork.PlayerList)
                    {
                        showScoreObject.GetComponent<TextComponent>().texts[i].text = p.NickName + ": " + p.CustomProperties["bestJump"];
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
                if (showScoreTime > 10)
                {
                    Hashtable hash = new Hashtable();
                    hash.Add("showScore", 0);
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
                        bg.SetActive(false);
                        pendulum.SetActive(false);
                        transform.position = new Vector2(transform.position.x + (speed + speedBoost) * Time.deltaTime, transform.position.y + ySpeed * Time.deltaTime);

                        if (transform.position.y > 4.17f)
                        {
                            index = 3;
                            pv.RPC("changeSprite", RpcTarget.All, index + (5 * character));
                            canRecieveInput = false;
                        }
                        else if (transform.position.y > 3.18f && yVel > 0)//about to land thing
                        {
                            index = 2;
                            pv.RPC("changeSprite", RpcTarget.All, index + (5 * character));

                            Debug.Log("hi");
                            if (canRecieveInput)
                            {


                                if (Input.GetKey(KeyCode.Space)) inputDir = 0;
                                else inputDir = -1;
                                if (inputDir != -1)
                                {
                                    if (inputDir == 0)
                                    {

                                        speedBoost += 1 / ((speedDamper * (fullTimer)) + .25f);//this makes it so that the closer you are to the when the button shows up (which happens after the first animation frame ie swap time being used) the better jump Multiplier you get
                                    }
                                    else
                                    {
                                        speedBoost /= 2;
                                    }

                                    canRecieveInput = false;
                                }
                            }
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
                                if (Mathf.Round(transform.position.x * 100) / 100 > (float)PhotonNetwork.LocalPlayer.CustomProperties["bestJump"])
                                {
                                    Hashtable hash = new Hashtable();
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
                    boostText.text = "Boost: " + Mathf.RoundToInt(speedBoost * 1000f);
                }
                
            }
            else if (!shouldStart)
            {
                if (cam.gameObject.activeInHierarchy && Input.GetKey(KeyCode.Space) && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("currentPlayer") && (int)PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"] == PhotonNetwork.LocalPlayer.ActorNumber) shouldStart = true;
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("currentPlayer") && (int)PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"] == PhotonNetwork.LocalPlayer.ActorNumber && transform.position.x == -1.38f) 
                {
                    bg.SetActive(true);
                    boostText.text = "Boost: " + Mathf.RoundToInt(speedBoost * 1000f);
                }
                else
                {
                    bg.SetActive(false);
                    boostText.text = " ";
                    
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
                boostText.text = "Distance: " + Mathf.Round(transform.position.x * 100) / 100; //rounded to two decimal places
                if (!hasChangedIndex)
                {
                    Hashtable hash = new Hashtable();
                    if ((int)PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"] + 1 > PhotonNetwork.CurrentRoom.PlayerCount)
                    {
                        hash.Add("jumpCount", (int)PhotonNetwork.CurrentRoom.CustomProperties["jumpCount"] + 1);
                        hash.Add("currentPlayer", 1);
                        if ((int)PhotonNetwork.CurrentRoom.CustomProperties["showScore"] == 0) {
                            
                            hash.Add("showScore", 1);
                        } 


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
                    transform.position = new Vector3(-1.38f, 3.17f, -100f);
                    showScoreObject.SetActive(false);
                    Hashtable hash = new Hashtable();
                    Vector3 ang = pendulum.transform.eulerAngles;
                    ang.z = 90;
                    pendulum.transform.rotation = Quaternion.Euler(ang);
                    PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
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
    

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
    }

}
