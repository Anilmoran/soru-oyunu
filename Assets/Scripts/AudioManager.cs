using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource MuzikKaynagi;
    public AudioSource EfektKaynagi;
    public AudioSource TikTakKaynagi;

    public AudioClip SahneMuzigi;
    public AudioClip ButonHoverSesi;
    public AudioClip ButonClickSesi;
    public AudioClip TikTakSesi;

    [Header("Cevap ve Joker Sesleri")]
    public AudioClip DogruCevapSesi;
    public AudioClip YanlisCevapSesi;
    public AudioClip JokerSesi; // YENÝ EKLEDÝÐÝMÝZ JOKER SESÝ DEÐÝÞKENÝ

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (MuzikKaynagi != null && SahneMuzigi != null)
        {
            MuzikKaynagi.clip = SahneMuzigi;
            MuzikKaynagi.loop = true;
            MuzikKaynagi.Play();
        }
    }

    public void HoverSesiCal() { if (EfektKaynagi != null && ButonHoverSesi != null) EfektKaynagi.PlayOneShot(ButonHoverSesi); }
    public void ClickSesiCal() { if (EfektKaynagi != null && ButonClickSesi != null) EfektKaynagi.PlayOneShot(ButonClickSesi); }
    public void TikTakCal() { if (TikTakKaynagi != null && TikTakSesi != null) TikTakKaynagi.PlayOneShot(TikTakSesi); }

    public void DogruSesiCal() { if (EfektKaynagi != null && DogruCevapSesi != null) EfektKaynagi.PlayOneShot(DogruCevapSesi); }
    public void YanlisSesiCal() { if (EfektKaynagi != null && YanlisCevapSesi != null) EfektKaynagi.PlayOneShot(YanlisCevapSesi); }

    // YENÝ EKLEDÝÐÝMÝZ JOKER ÇALMA FONKSÝYONU
    public void JokerSesiCal() { if (EfektKaynagi != null && JokerSesi != null) EfektKaynagi.PlayOneShot(JokerSesi); }
}