using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource MuzikKaynagi;
    public AudioSource EfektKaynagi;
    public AudioSource TikTakKaynagi; // Geri sayým ses kaynaðý

    public AudioClip SahneMuzigi;
    public AudioClip ButonHoverSesi;
    public AudioClip ButonClickSesi;

    [Header("Süre Sesleri")]
    public AudioClip TikTakSesi;       // Senin 25 saniyelik normal sesin
    public AudioClip TikTakHizliSesi;  // YENÝ EKLENEN 10 saniyelik hýzlý ses

    [Header("Cevap ve Joker Sesleri")]
    public AudioClip DogruCevapSesi;
    public AudioClip YanlisCevapSesi;
    public AudioClip JokerSesi;

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

    public void DogruSesiCal() { if (EfektKaynagi != null && DogruCevapSesi != null) EfektKaynagi.PlayOneShot(DogruCevapSesi); }
    public void YanlisSesiCal() { if (EfektKaynagi != null && YanlisCevapSesi != null) EfektKaynagi.PlayOneShot(YanlisCevapSesi); }
    public void JokerSesiCal() { if (EfektKaynagi != null && JokerSesi != null) EfektKaynagi.PlayOneShot(JokerSesi); }

    // --- YENÝ GERÝ SAYIM SES SÝSTEMÝ ---
    public void TikTakNormalBaslat()
    {
        if (TikTakKaynagi != null && TikTakSesi != null)
        {
            TikTakKaynagi.clip = TikTakSesi;
            TikTakKaynagi.loop = true; // Sesi döngüye alýyoruz (25 saniye bitince kendi baþa saracak)
            TikTakKaynagi.Play();
        }
    }

    public void TikTakHizliBaslat()
    {
        if (TikTakKaynagi != null && TikTakHizliSesi != null)
        {
            TikTakKaynagi.clip = TikTakHizliSesi;
            TikTakKaynagi.loop = true;
            TikTakKaynagi.Play();
        }
    }

    public void TikTakDurdur()
    {
        if (TikTakKaynagi != null) TikTakKaynagi.Stop();
    }
}