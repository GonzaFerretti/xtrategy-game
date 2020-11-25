using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] Color centerColor;
    [SerializeField] Color sideColor;
    [SerializeField] float time;
    [SerializeField] Renderer rend;

    public void Setup(bool isCenter)
    {
        Material material = rend.material;
        Material newMat = Instantiate(material);

        newMat.SetColor("_Color", isCenter ? centerColor : sideColor);

        rend.material = newMat;

        StartCoroutine(DecayAfterTime());
    }

    IEnumerator DecayAfterTime()
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
