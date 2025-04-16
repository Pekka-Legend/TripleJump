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

public class PlayerScript : MonoBehaviour
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
    public Player activePlayer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boostText = GameObject.FindGameObjectWithTag("boost").GetComponent<Text>();
        key = FindFirstObjectByType<TextMeshProUGUI>();
        bg = FindFirstObjectByType<Canvas>().gameObject.GetComponentInChildren<Image>().gameObject;
        pv = GetComponent<PhotonView>();

    }

    // Update is called once per frame
    void Update()//-1.38f->22.96f
    {
        if (pv.IsMine)
        {
            if (shouldStart && !shouldEnd)
            {
                if (activePlayer != null && activePlayer.IsLocal)
                {
                    
                    if (steps >= 14)
                    {
                        transform.position = new Vector2(transform.position.x + (speed + speedBoost) * Time.deltaTime, transform.position.y + ySpeed * Time.deltaTime);

                        if (transform.position.y > 4.17f)
                        {
                            index = 3;
                            pv.RPC("changeSprite", RpcTarget.All, index);
                            canRecieveInput = false;
                            bg.SetActive(false);
                            key.text = " ";
                        }
                        else if (transform.position.y > 3.18f && yVel > 0)
                        {
                            index = 2;
                            pv.RPC("changeSprite", RpcTarget.All, index);
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
                            pv.RPC("changeSprite", RpcTarget.All, index);
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
                            pv.RPC("changeSprite", RpcTarget.All, index);
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

                if (Input.GetKeyDown(KeyCode.W) && activePlayer != null && activePlayer.IsLocal) shouldStart = true;
                
                if (PhotonNetwork.IsMasterClient)
                {
                    if (PhotonNetwork.LocalPlayer.CustomProperties["currentPlayer"] != null)
                    {
                        pv.RPC("start", RpcTarget.All, PhotonNetwork.PlayerList[(int)PhotonNetwork.LocalPlayer.CustomProperties["currentPlayer"]]);
                        pv.RPC("SetActiveCamera", RpcTarget.All, PhotonNetwork.PlayerList[(int)PhotonNetwork.LocalPlayer.CustomProperties["currentPlayer"]].ActorNumber);
                    }
                }

            }
            else if (shouldEnd && activePlayer != null && activePlayer.IsLocal)
            {
                index = 4;
                pv.RPC("changeSprite", RpcTarget.All, index);
                boostText.text = "Distance: " + Mathf.Round(transform.position.x * 100) / 100; //rounded to two decimal places
            }
        }
        if (PhotonNetwork.LocalPlayer.CustomProperties["currentPlayer"] != null) Debug.Log((int)PhotonNetwork.LocalPlayer.CustomProperties["currentPlayer"]);


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
    public void start(Player player)
    {
        
        activePlayer = player;
        Debug.Log("hi");
    }
    [PunRPC]
    public void nextPlayer(Player player)
    {
        activePlayer = player;

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

}
