using UnityEngine;
using TMPro;
using System.Collections;

public class SoruManager : MonoBehaviour
{
    public static SoruManager Instance; // Artýk SoruManager da bir patron!

    [Header("UI Elementleri")]
    public TextMeshProUGUI SureYazisi;

    private int kalanSure = 60;
    private Coroutine sureRutini;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Baþlangýçta GameManager soruyu getirene kadar süreyi baþlatmýyoruz
    }

    public void SuresiBaslat()
    {
        kalanSure = 60;
        if (SureYazisi != null) { SureYazisi.text = kalanSure.ToString(); SureYazisi.color = Color.white; }

        // SÜRE BAÞLARKEN NORMAL SESÝ ÇAL (Kendi kendine loop döner)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.TikTakNormalBaslat();
        }

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

            // SÜRE 10 SANÝYEYE ÝNDÝÐÝNDE HIZLI SESÝ BAÞLAT
            if (kalanSure == 10)
            {
                if (SureYazisi != null) SureYazisi.color = Color.red;

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.TikTakHizliBaslat();
                }
            }
        }

        // SÜRE SIFIRLANINCA SESÝ SUSTUR VE ELENME ÝÞLEMÝNÝ BAÞLAT
        if (SureYazisi != null) SureYazisi.text = "SÜRE BÝTTÝ!";
        if (AudioManager.Instance != null) AudioManager.Instance.TikTakDurdur();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SureBittiElendi();
        }
    }

    // Doðru cevaplandýðýnda veya elenildiðinde süreyi/sesi durdurur
    public void SuresiDurdur()
    {
        if (sureRutini != null) StopCoroutine(sureRutini);
        if (AudioManager.Instance != null) AudioManager.Instance.TikTakDurdur();
    }
}