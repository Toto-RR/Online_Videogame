using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TakeDamageUI : MonoBehaviour
{
    public float showTime = 5f;
    public Image takeDamage;

    public void GetTakedamage()
    {
        StopCoroutine("showTakeDamage");
        takeDamage.enabled = true;
        StartCoroutine("showTakeDamage");
    }

    public IEnumerator showTakeDamage()
    {
        yield return new WaitForSeconds(showTime);
        takeDamage.enabled = false;
    }
}
