using TMPro;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public Sprite[] sprites;
    private float timer = 0;
    public float swapTime = 0;
    private int index = 0;
    public GameObject bg;
    public TextMeshProUGUI key;
    private bool moveDir = false; //false = left, true = right
    private float fullTimer = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (index >= 1)// this will be messed up when I go to add in the actual jump part
        {
            bg.SetActive(true);
            if (moveDir) key.text = "A";
            else key.text = "D";
            //in here do some sort of speed calculation and detect actual button presses
            //probably do 3 "run throughs" before the competition to guage the player's average run through distance (this will change based on their step timing)
            //could also just slow down the animation speed but that would make it look bad
            //or just have it only impact jump height (probably easiest and best)
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
                moveDir = !moveDir;
                fullTimer = 0;
            }
            GetComponent<SpriteRenderer>().sprite = sprites[index];
            timer = 0;
        }
        timer += Time.deltaTime;
        fullTimer += Time.deltaTime;

    }
}
