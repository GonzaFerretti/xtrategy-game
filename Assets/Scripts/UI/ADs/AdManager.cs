using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour
{
    string _adId = "3908583";
    string lastType;

    [SerializeField] float timeAfterAd;

    [SerializeField] GameObject ImageAd;
    [SerializeField] GameObject VideoAd;
    [SerializeField] HUDManager hud;

    public bool isEnabled = false;

    [SerializeField] GameObject MainContainer;

    private void Start()
    {
        Advertisement.Initialize(_adId, false);
    }

    public void ToggleVisibility()
    {
        isEnabled = !isEnabled;
        MainContainer.SetActive(isEnabled);
    }

    public void ShowAd(string adtype)
    {
        if (Advertisement.IsReady())
        {
            lastType = adtype;
            Advertisement.Show(adtype, new ShowOptions() { resultCallback = HandleResult});
        }
    }

    public void HandleResult(ShowResult result)
    {
        if (result == ShowResult.Finished)
        {
            HideOptions();
            ShowPrize();
            StartCoroutine(StartGiveBonusTimer());
        }/*
        else
        {
            ShowTryAgain();
        }*/
    }

    IEnumerator StartGiveBonusTimer()
    {
        yield return new WaitForSeconds(timeAfterAd);
        ImageAd.SetActive(false);
        VideoAd.SetActive(false);
        hud.gm.UsePower(lastType);
    }

    private void HideOptions()
    {
        MainContainer.SetActive(false);
    }

    private void ShowPrize()
    {
        if (lastType == "video")
        {
            VideoAd.SetActive(true);
            ImageAd.SetActive(false);
        }
        else
        {
            ImageAd.SetActive(true);
            VideoAd.SetActive(false);
        }
    }
}
public enum AdType
{
    video,
    staticBanner,
}
