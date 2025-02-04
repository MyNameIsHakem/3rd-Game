using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LvsManager : MonoBehaviour
{
    public Transform Sliders;
    public TextMeshProUGUI CollectedStars;
    public Animator LoadScreen;

    private int TotalStars;

    public void LoadLvsData()
    {
        TotalStars = 0;

        int LvsPerSlider = Sliders.GetChild(0).childCount;

        ShowLvStars(Sliders.GetChild(0).GetChild(0).GetChild(1), 1);

        for (int i = 1; i < PlayerData.CurrentLv; i++)
        {
            Transform CurLv = Sliders.GetChild(i / LvsPerSlider).GetChild(i % LvsPerSlider);

            CurLv.GetChild(1).gameObject.SetActive(false);

            ShowLvStars(CurLv.GetChild(2), i+1);            
        }

        CollectedStars.text = TotalStars.ToString();
    }

    void ShowLvStars(Transform Stars, int lvNum)
    {
        TotalStars += PlayerData.LvXStars[lvNum];

        for (int i = 0; i < PlayerData.LvXStars[lvNum]; i++)
        {           
            Stars.GetChild(i).gameObject.SetActive(true);
        }
    }

    public void ChooseLv(TextMeshProUGUI Lv)
    {
        int lv = int.Parse(Lv.text);

        if(lv <= PlayerData.CurrentLv)
        {
            AdsManager.HideBanner();
            LoadScreen.gameObject.SetActive(true);
            LoadScreen.SetTrigger("Appear");
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(lv);
        }
        
    }


}
