using System;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

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
    public float multiplyDamper = 0;
    private float inputDir = -1;//-1 is no input (all others same as moveDir)
    private bool canRecieveInput = true;
    public float gravity = 9.8f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()//-1.38f->22.96f
    {
        Debug.Log(jumpMultiplier);
        if (shouldStart)
        {
            if (steps >= 14)
            {
                transform.position = new Vector2(transform.position.x + speed * Time.deltaTime, transform.position.y + ySpeed * Time.deltaTime);
                if (transform.position.y > 3.17)
                {
                    index = 3;
                    GetComponent<SpriteRenderer>().sprite = sprites[index];
                }
                else
                {
                    transform.position = new Vector2(transform.position.x, 3.17f);
                    ySpeed = jumpMultiplier;
                }
                ySpeed-= gravity * Time.deltaTime;
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
                            
                            jumpMultiplier += 1 / ((fullTimer - swapTime) + .25f);//this makes it so that the closer you are to the when the button shows up (which happens after the first animation frame ie swap time being used) the better jump Multiplier you get
                        }
                        else
                        {
                            jumpMultiplier /= 2;
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
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.W)) shouldStart = true;
        }

        

    }
}
