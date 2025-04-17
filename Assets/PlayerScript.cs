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
    public TextMeshProUGUI key;
    private int moveDir = 0; //0 = left, 1 = right, 2 = up, 3 = down
    private float fullTimer = 0;
    private float steps = 0;
    public float speed = 0;
    private bool shouldStart = false;
    private float ySpeed = 0;
    public float jumpForce = 0;
    private float inputDir = -1;//-1 is no input (all others same as moveDir)
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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boostText = GameObject.FindGameObjectWithTag("boost").GetComponent<Text>();
        
        pv = GetComponent<PhotonView>();
        if (pv.IsMine)
        {
            key = FindFirstObjectByType<TextMeshProUGUI>();
            bg = GameObject.FindGameObjectWithTag("tbg");
            showScoreObject = GameObject.FindGameObjectWithTag("sbg");
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
                key.text = " ";
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
                        transform.position = new Vector2(transform.position.x + (speed + speedBoost) * Time.deltaTime, transform.position.y + ySpeed * Time.deltaTime);

                        if (transform.position.y > 4.17f)
                        {
                            index = 3;
                            pv.RPC("changeSprite", RpcTarget.All, index + (5 * character));
                            canRecieveInput = false;
                            bg.SetActive(false);
                            key.text = " ";
                        }
                        else if (transform.position.y > 3.18f && yVel > 0)
                        {
                            index = 2;
                            pv.RPC("changeSprite", RpcTarget.All, index + (5 * character));
                            if (canRecieveInput)
                            {
                                Debug.Log("hi");
                                bg.SetActive(true);
                                if (moveDir == 0) key.text = "A";
                                else if (moveDir == 1) key.text = "D";
                                else if (moveDir == 2) key.text = "W";
                                else if (moveDir == 3) key.text = "S";


                                if (Input.GetKey(KeyCode.A)) inputDir = 0;
                                else if (Input.GetKey(KeyCode.D)) inputDir = 1;
                                else if (Input.GetKey(KeyCode.W)) inputDir = 2;
                                else if (Input.GetKey(KeyCode.S)) inputDir = 3;
                                else inputDir = -1;
                                if (inputDir != -1)
                                {
                                    Debug.Log(fullTimer);
                                    if (inputDir == moveDir)
                                    {

                                        speedBoost += 1 / ((speedDamper * (fullTimer)) + .25f);//this makes it so that the closer you are to the when the button shows up (which happens after the first animation frame ie swap time being used) the better jump Multiplier you get
                                    }
                                    else
                                    {
                                        speedBoost /= 2;
                                    }

                                    canRecieveInput = false;
                                    bg.SetActive(false);
                                    key.text = " ";
                                }
                            }
                        }
                        else if (transform.position.y > 3.17f)
                        {
                            index = 0;
                            pv.RPC("changeSprite", RpcTarget.All, index + (5 * character));
                            bg.SetActive(false);
                            key.text = " ";
                            moveDir = (int)Random.Range(0f, 3f);
                            canRecieveInput = true;
                        }
                        else
                        {
                            transform.position = new Vector2(transform.position.x, 3.17f);
                            yVel = jumpForce;
                            ySpeed = 0;
                            jumps++;
                            bg.SetActive(false);
                            key.text = " ";
                            fullTimer = 0;
                            if (jumps == 4)//you land after the third jump
                            {
                                shouldEnd = true;
                                transform.position = new Vector2(transform.position.x, 2.92f);
                                if (Mathf.RoundToInt(speedBoost * 1000f) > (float)PhotonNetwork.LocalPlayer.CustomProperties["bestJump"])
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
                        if (index >= 0 && canRecieveInput && steps % 2 == 0)//make some sort of variable called "input was processed" for each step
                        {
                            bg.SetActive(true);
                            if (moveDir == 0) key.text = "A";
                            else if (moveDir == 1) key.text = "D";
                            else if (moveDir == 2) key.text = "W";
                            else if (moveDir == 3) key.text = "S";


                            if (Input.GetKey(KeyCode.A)) inputDir = 0;
                            else if (Input.GetKey(KeyCode.D)) inputDir = 1;
                            else if (Input.GetKey(KeyCode.W)) inputDir = 2;
                            else if (Input.GetKey(KeyCode.S)) inputDir = 3;
                            else inputDir = -1;
                            if (inputDir != -1)
                            {
                                if (inputDir == moveDir)
                                {

                                    speedBoost += 1 / ((speedDamper * (fullTimer - swapTime)) + .25f);//this makes it so that the closer you are to the when the button shows up (which happens after the first animation frame ie swap time being used) the better jump Multiplier you get
                                }
                                else
                                {
                                    speedBoost /= 2;
                                }

                                canRecieveInput = false;
                            }

                        }
                        else
                        {
                            bg.SetActive(false);
                            key.text = " ";
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
                                    moveDir = (int)Random.Range(0f, 3f);
                                    canRecieveInput = true;
                                }



                            }
                            pv.RPC("changeSprite", RpcTarget.All, index + (5 * character));
                            timer = 0;


                        }
                        timer += Time.deltaTime;
                        fullTimer += Time.deltaTime;
                        transform.position = new Vector2(transform.position.x + speed * Time.deltaTime, transform.position.y);
                    }
                    boostText.text = "Boost: " + Mathf.RoundToInt(speedBoost * 1000f);
                    Debug.Log("From Cam: " + transform.position);
                }
                
            }
            else if (!shouldStart)
            {
                Debug.Log("AN: " + PhotonNetwork.LocalPlayer.ActorNumber);
                if (cam.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.W) && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("currentPlayer") && (int)PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"] == PhotonNetwork.LocalPlayer.ActorNumber) shouldStart = true;
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("currentPlayer") && (int)PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"] == PhotonNetwork.LocalPlayer.ActorNumber && transform.position.x == -1.38f) 
                {
                    bg.SetActive(true);
                    key.text = "W";
                    boostText.text = "Boost: " + Mathf.RoundToInt(speedBoost * 1000f);
                }
                else
                {
                    bg.SetActive(false);
                    key.text = " ";
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
                Debug.Log(transform.position.x);
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
                    moveDir = 0; //0 = left, 1 = right, 2 = up, 3 = down
                    fullTimer = 0;
                    steps = 0;
                    shouldStart = false;
                    ySpeed = 0;
                    inputDir = -1;//-1 is no input (all others same as moveDir)
                    canRecieveInput = true;
                    yVel = 0;
                    speedBoost = 0;
                    jumps = 0;
                    shouldEnd = false;
                    leaveTime = 0;
                    shouldChangeActivePlayer = 0;
                    transform.position = new Vector3(-1.38f, 3.17f, -100f);
                    showScoreObject.SetActive(false);
                    Hashtable hash = new Hashtable();
                    PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                    pv.RPC("masterChangeActivePlayer", RpcTarget.All);
                    
                }
                
                leaveTime += Time.deltaTime;
            }
            if (PhotonNetwork.IsMasterClient && shouldChangeActivePlayer == 1)
            {
                pv.RPC("SetActiveCamera", RpcTarget.All, (int)PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"]);
                shouldChangeActivePlayer = 0;
                Debug.Log("Changed active player");
            }
        }
        if (PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"] != null) Debug.Log((int)PhotonNetwork.CurrentRoom.CustomProperties["currentPlayer"]);


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
