using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    enum HideStates
    {
        hidden,
        waitingToUnhide,
        interruptingUnhiding,
        visible,
    }

    public Material transBaseMat;
    Animator anim;
    MeshRenderer meshren;
    Material[] usualMat;
    Material[] transMat;
    HideStates currentHideState = HideStates.visible;

    [Range(0, 1)]
    public float opacity;
    public float lastOpacity;

    public void Start()
    {
        anim = GetComponent<Animator>();
        meshren = GetComponent<MeshRenderer>();
        usualMat = meshren.materials;
        transMat = new Material[meshren.materials.Length];
        for (int i = 0; i < meshren.materials.Length; i++)
        {
            Material mat = new Material(transBaseMat);
            mat.SetTexture("_albedo", usualMat[i].GetTexture("_MainTex"));
            mat.SetTexture("_normal", usualMat[i].GetTexture("_BumpMap"));
            mat.SetTexture("_metalicSmoothness", usualMat[i].GetTexture("_MetallicGlossMap"));
            mat.SetTexture("_ao", usualMat[i].GetTexture("_OcclusionMap"));
            transMat[i] = mat;
        }
    }

    public void Hide(float time)
    {
        if (currentHideState == HideStates.visible)
        {
            anim.SetBool("isHiding", true);
            meshren.materials = transMat;
            StartCoroutine(StartTimerToUnHideAgain(time));
            currentHideState = HideStates.waitingToUnhide;
        }
        else if (currentHideState == HideStates.waitingToUnhide)
        {
            currentHideState = HideStates.interruptingUnhiding;
        }
    }

    IEnumerator StartTimerToUnHideAgain(float time)
    {
        float finalTime = Time.time + time;
        while (Time.time < finalTime)
        {
            yield return new WaitForEndOfFrame();
            if (currentHideState == HideStates.interruptingUnhiding)
            {
                currentHideState = HideStates.waitingToUnhide;
                StartCoroutine(StartTimerToUnHideAgain(time));
                yield break;
            }
        }
        currentHideState = HideStates.visible;
        anim.SetBool("isHiding", false);
    }

    public void Update()
    {
        UpdateOpacity();
    }

    public void UpdateOpacity()
    {
        if (lastOpacity != opacity)
        {
            foreach (Material mat in meshren.materials)
            {
                mat.SetFloat("_opacity", opacity);
            }
            if (opacity == 1)
            {
                meshren.materials = usualMat;
            }
        }
        lastOpacity = opacity;
    }
}
