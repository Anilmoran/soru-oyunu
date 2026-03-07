using UnityEngine;
using TMPro;
using System.Collections;

public class SoruManager : MonoBehaviour
{
    [Header("UI Elementleri")]
    public TextMeshProUGUI SureYazisi;

    private int kalanSure = 60;
    private Coroutine sureRutini;

    void Start()
    {
        SuresiBaslat();
    }

    public void SuresiBaslat()
    {
        kalanSure = 60;
        if (SureYazisi != null) { SureYazisi.text = kalanSure.ToString(); SureYazisi.color = Color.white; }

        if (sureRutini != null) StopCoroutine(sureRutini);
        sureRutini = StartCoroutine(ZamanGeriSayimi());
    }

    IEnumerator ZamanGeriSayimi()
    {
        while (kalanSure > 0)
        {
            yield return new WaitForSeconds(1f);
            kalanSure--;

            if (SureYazisi != null) SureYazisi.text = kalanSure.ToString();

            // YENÝ SÝSTEM: Sürükle-býrak yok, direkt Sahne Patronuna emir veriyoruz!
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.TikTakCal();
            }

            // Son 10 saniyede yazý kýrmýzý olsun
            if (kalanSure <= 10 && SureYazisi != null)
            {
                SureYazisi.color = Color.red;
            }
        }

        // SÜRE BÝTÝNCE:
        if (SureYazisi != null) SureYazisi.text = "SÜRE BÝTTÝ!";
    }
}