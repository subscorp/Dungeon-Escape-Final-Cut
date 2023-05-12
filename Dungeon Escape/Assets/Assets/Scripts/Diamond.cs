using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diamond : MonoBehaviour
{
    [SerializeField]
    private int _val = 1;

    public Diamond(int val)
    {
        _val = val;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.name == "Player")
        {
            Debug.Log("Player Collected diamond using OnTriggerEnter2D");
            Player player = other.GetComponent<Player>();
            AudioManager.Instance.PlayGettingCollectibleSFX();
            player.AddGems(_val);
            Destroy(gameObject);
        }
    }

    /*private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.name == "Player")
        {
            Debug.Log("Player Collected diamond using OnColissionEnter2D");
            Player player = other.collider.GetComponent<Player>();
            AudioManager.Instance.PlayGettingCollectibleSFX();
            player.AddGems(_val);
            Destroy(gameObject);
        }
    }*/

    public void SetVal(int val)
    {
        _val = val;
    }

    public void SetScale(float scale)
    {
        transform.localScale = new Vector3(scale, scale, 1);
    }
}
