using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    int health;
    Player player;
    GameObject[] life;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        health = Player.health;
        life = new GameObject[100];
        life[1] = this.transform.GetChild(0).gameObject;
        UpdateHealthDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        if (health != Player.health)
        {
            for (int i = 2; i <= health; i++)
            {
                Destroy(life[i]);
            }
            health = Player.health;
            UpdateHealthDisplay();
        }
    }

    void UpdateHealthDisplay()
    {
        if (health > 0)
        {
            for (int i = 2; i <= health; i++)
            {
                life[i] = Instantiate(life[i - 1], new Vector3(0, 0, 0), Quaternion.identity);
                life[i].name = "Life" + i;
                life[i].transform.parent = gameObject.transform;
                life[i].transform.localPosition = new Vector3(life[i - 1].transform.localPosition.x + life[i - 1].transform.localScale.x + 0.2f, life[i - 1].transform.localPosition.y, life[i - 1].transform.localPosition.z);
                life[i].transform.SetParent(life[i - 1].transform.parent);
            }
        }
        else
        {
            Destroy(life[1]);
        }
    }
}
