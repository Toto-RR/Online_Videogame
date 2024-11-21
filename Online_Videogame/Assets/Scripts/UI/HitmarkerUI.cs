using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitmarkerUI : MonoBehaviour
{
    public float showTime = 5f;
    public Image hitMarker;

    public void GetHitmarker()
    {
        StopCoroutine("showHitmarker");
        hitMarker.enabled = true;
        StartCoroutine("showHitmarker");
    }

    public IEnumerator showHitmarker()
    {
        yield return new WaitForSeconds(showTime);
        hitMarker.enabled = false;
    }
}
