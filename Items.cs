using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour
{

    GameObject playerGameObject;

    // Start is called before the first frame update
    void Start()
    {
        if (Player.doubleJump == true)
        {
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (this.CompareTag("DoubleJump")){
                Player.doubleJump = true;
                GameState.I.doubleJump = true;
            }
            Destroy(this.gameObject);
        }
    }
}
