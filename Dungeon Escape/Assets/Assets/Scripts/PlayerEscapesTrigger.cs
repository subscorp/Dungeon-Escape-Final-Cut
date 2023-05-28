using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEscapesTrigger : MonoBehaviour
{
    private Animator _bossAnimator;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!GameManager.Instance.BossDead)
        {
            GameObject _bossObject = GameObject.FindGameObjectWithTag("Boss");
            if (_bossObject != null)
            {
                _bossAnimator = _bossObject.GetComponentInChildren<Animator>();
                _bossAnimator.SetTrigger("Player_Escapes");
            }
            GameManager.Instance.StartedBossFight = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameManager.Instance.StartedBossFight = false;
    }
}
