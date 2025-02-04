using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using TMPro;

public class LoadData : MonoBehaviour
{
    public TextMeshProUGUI MoneyDis;
    public ShopManager ShopMan;
    public SettingsManager SettingsMan;
    public LvsManager LvsMan;
    public GameObject VibrationPanel;

    [Space]
    [Tooltip("How Much Delay Between each Banner Loaded Check and if it's Loaded I Show the banner")]
    public float BannerDelay;

    void Awake()
    {
        //SaveSystem.Load();

        if (PlayerData.FirstTime)
        {
            VibrationPanel.SetActive(true);
        }
        else
        {
            Destroy(VibrationPanel);
        }

        MoneyDis.text = PlayerData.Money.ToString();

        ShopMan.LoadBoughtItems();        

        LvsMan.LoadLvsData();                 
    }

    void Start()
    {
        //I Put this On "Start" Bacause I can't change the AudioMixer value on "Awake" (Unity Problem)
        SettingsMan.LoadSettings();

        StartCoroutine(StartBanner(AdTypes.Banner_Android));
    }

    IEnumerator StartBanner(AdTypes Bannertype)
    {
        //Wait Until the Player set the Vibration he wants
        yield return new WaitUntil(() => !PlayerData.FirstTime);

        do
        {
            yield return new WaitForSeconds(BannerDelay);

            AdsManager.StartAd(Bannertype);

        } while (!Advertisement.Banner.isLoaded);

    }

    public void VibrationPanelEnd()
    {        
        Destroy(VibrationPanel);
        PlayerData.FirstTime = false;
        SaveSystem.Save();

        SettingsMan.LoadSettings();
    }
}
