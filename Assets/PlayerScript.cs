using System;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using Photon.Pun;

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
    private float jumpMultiplier = 1;
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boostText = FindFirstObjectByType<Text>();
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
                if (steps >= 14)
                {
                    transform.position = new Vector2(transform.position.x + (speed + speedBoost) * Time.deltaTime, transform.position.y + ySpeed * Time.deltaTime);

                    if (transform.position.y > 4.17f)
                    {
                        index = 3;
                        GetComponent<SpriteRenderer>().sprite = sprites[index];
                        canRecieveInput = false;
                        bg.SetActive(false);
                        key.text = " ";
                    }
                    else if (transform.position.y > 3.18f && yVel > 0)
                    {
                        index = 2;
                        GetComponent<SpriteRenderer>().sprite = sprites[index];
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
                        GetComponent<SpriteRenderer>().sprite = sprites[index];
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
                        GetComponent<SpriteRenderer>().sprite = sprites[index];
                        timer = 0;


                    }
                    timer += Time.deltaTime;
                    fullTimer += Time.deltaTime;
                    transform.position = new Vector2(transform.position.x + speed * Time.deltaTime, transform.position.y);
                }
                boostText.text = "Boost: " + Mathf.RoundToInt(speedBoost * 1000f);
            }
            else if (!shouldStart)
            {

                if (Input.GetKeyDown(KeyCode.W)) shouldStart = true;
            }
            else if (shouldEnd)
            {
                index = 4;
                GetComponent<SpriteRenderer>().sprite = sprites[index];
                boostText.text = "Distance: " + Mathf.Round(transform.position.x * 100) / 100; //rounded to two decimal places
            }
        }
        else
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
        }
        

    }
}
