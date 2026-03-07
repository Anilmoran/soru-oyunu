using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // SAHNE DEÐÝÞTÝRMEK ÝÇÝN EKLENDÝ

[System.Serializable]
public class SoruVerisi { public string soru; public string a; public string b; public string c; public string d; public string dogruCevap; }

[System.Serializable] public class GroqResponse { public Choice[] choices; }
[System.Serializable] public class Choice { public Message message; }
[System.Serializable] public class Message { public string content; }

public class GameManager : MonoBehaviour
{
    [Header("Soru ve Þýklar")]
    public TextMeshProUGUI SoruText;
    public TextMeshProUGUI[] SecenekTexts;
    public Button[] SecenekBtns;

    [Header("Joker Butonlarý")]
    public Button Joker50_Btn;
    public Button ciftcevapBtn;
    public Button seyirciBtn;

    [Header("Oyun Sonu Paneli")]
    public GameObject KazandiPaneli;
    public TextMeshProUGUI SiralamaText;
    public Button AnaMenu_Btn;
    public Button LobiyeDon_Btn;
    public Button OyundanCik_Btn;

    // GROQ ÞÝFRENÝ BURAYA YAPIÞTIR!
    private string apiKey = "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";

    private string gecerliDogruCevap = "";
    private string aktifSoruMetni = "";
    private int soruSirasi = 1;
    private string[] kategoriler = { "Tarih", "Coðrafya", "Uzay Bilimi", "Sinema", "Spor", "Müzik", "Teknoloji", "Biyoloji", "Sanat Tarihi", "Edebiyat" };

    private bool joker50Kullanildi = false;
    private bool ciftCevapKullanildi = false;
    private bool seyirciKullanildi = false;
    private bool ciftCevapHakkiAktif = false;

    void Start()
    {
        // Oyun baþlarken panel kapalý olsun
        if (KazandiPaneli != null) KazandiPaneli.SetActive(false);

        ButonlariHazirla();
        soruSirasi = 1;
        YeniSoruGetir();
    }

    public void YeniSoruGetir()
    {
        // Soru sayýsýný 15'e çýkardýk
        if (soruSirasi > 15)
        {
            OyunBittiPaneliniAc();
            return;
        }

        ciftCevapHakkiAktif = false;
        aktifSoruMetni = soruSirasi + ". Soru Hazýrlanýyor...";
        SoruText.text = aktifSoruMetni;

        foreach (Button btn in SecenekBtns) btn.interactable = false;
        foreach (TextMeshProUGUI txt in SecenekTexts) txt.text = "...";

        StartCoroutine(YapayZekadanSoruCek());
    }

    IEnumerator YapayZekadanSoruCek()
    {
        string zorlukDerecesi = "kolay";
        if (soruSirasi >= 4 && soruSirasi <= 7) zorlukDerecesi = "orta";
        else if (soruSirasi >= 8) zorlukDerecesi = "çok zor";

        string rastgeleKategori = kategoriler[Random.Range(0, kategoriler.Length)];
        string prompt = "Bana " + rastgeleKategori + " kategorisinde 1 tane " + zorlukDerecesi + " seviye bilgi yarýþmasý sorusu ver. Daha önce çok sorulmamýþ, ilginç bir soru olsun. Sadece JSON formatýnda cevap ver. JSON anahtarlarý þunlar olsun: soru, a, b, c, d, dogruCevap. ÇOK ÖNEMLÝ: 'dogruCevap' anahtarýnýn karþýsýna SADECE doðru þýkkýn harfini yaz (a, b, c veya d). Baþka açýklama yapma.";

        string url = "https://api.groq.com/openai/v1/chat/completions";
        string jsonIstek = "{\"model\": \"llama-3.3-70b-versatile\", \"messages\": [{\"role\": \"user\", \"content\": \"" + prompt + "\"}], \"temperature\": 0.7}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonIstek);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            SoruText.text = "Baðlantý Hatasý!";
        }
        else
        {
            try
            {
                GroqResponse response = JsonUtility.FromJson<GroqResponse>(request.downloadHandler.text);
                if (response != null && response.choices != null && response.choices.Length > 0)
                {
                    string yapayZekaMetni = response.choices[0].message.content.Replace("```json", "").Replace("```", "").Trim();
                    SoruVerisi yeniSoru = JsonUtility.FromJson<SoruVerisi>(yapayZekaMetni);

                    gecerliDogruCevap = yeniSoru.dogruCevap.ToLower().Trim();
                    if (gecerliDogruCevap.Length > 1) gecerliDogruCevap = gecerliDogruCevap.Substring(0, 1);

                    aktifSoruMetni = "SORU " + soruSirasi + ":\n" + yeniSoru.soru;
                    SoruText.text = aktifSoruMetni;

                    SecenekTexts[0].text = "A) " + yeniSoru.a;
                    SecenekTexts[1].text = "B) " + yeniSoru.b;
                    SecenekTexts[2].text = "C) " + yeniSoru.c;
                    SecenekTexts[3].text = "D) " + yeniSoru.d;

                    foreach (Button btn in SecenekBtns) btn.interactable = true;
                }
            }
            catch { SoruText.text = "Soru formatý bozuk geldi."; }
        }
    }

    void ButonlariHazirla()
    {
        SecenekBtns[0].onClick.AddListener(() => CevapKontrol("a", 0));
        SecenekBtns[1].onClick.AddListener(() => CevapKontrol("b", 1));
        SecenekBtns[2].onClick.AddListener(() => CevapKontrol("c", 2));
        SecenekBtns[3].onClick.AddListener(() => CevapKontrol("d", 3));

        Joker50_Btn.onClick.AddListener(Joker50Kullan);
        ciftcevapBtn.onClick.AddListener(JokerCiftCevapKullan);
        seyirciBtn.onClick.AddListener(JokerSeyirciKullan);

        // OYUN SONU BUTONLARI
        if (AnaMenu_Btn != null) AnaMenu_Btn.onClick.AddListener(AnaMenuyeGit);
        if (OyundanCik_Btn != null) OyundanCik_Btn.onClick.AddListener(OyundanCik);
        if (LobiyeDon_Btn != null) LobiyeDon_Btn.onClick.AddListener(LobiyeGit);
    }

    public void CevapKontrol(string secilenSik, int butonIndex)
    {
        if (secilenSik == gecerliDogruCevap)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.DogruSesiCal();

            ciftCevapHakkiAktif = false;
            soruSirasi++;
            SoruText.text = aktifSoruMetni + "\n\n<color=green>DOÐRU BÝLDÝN! Sýradaki soru hazýrlanýyor...</color>";
            YeniSoruGetir();
        }
        else
        {
            if (AudioManager.Instance != null) AudioManager.Instance.YanlisSesiCal();

            if (ciftCevapHakkiAktif)
            {
                ciftCevapHakkiAktif = false;
                SecenekBtns[butonIndex].interactable = false;
                SoruText.text = aktifSoruMetni + "\n\n<color=orange>ÝLK TAHMÝNÝN YANLIÞ! Kalkan seni korudu, kalan 3 þýktan tekrar seçim yap!</color>";
            }
            else
            {
                SoruText.text = aktifSoruMetni + "\n\n<color=red>YANLIÞ CEVAP! Maalesef elendin.</color>";
                foreach (Button btn in SecenekBtns) btn.interactable = false;

                // Elenirse 2 saniye sonra DNF panelini aç
                Invoke("OyunBittiPaneliniAc", 2f);
            }
        }
    }

    public void Joker50Kullan()
    {
        if (joker50Kullanildi) return;
        joker50Kullanildi = true;
        Joker50_Btn.interactable = false;

        if (AudioManager.Instance != null) AudioManager.Instance.JokerSesiCal();

        int dogruIndex = 0;
        if (gecerliDogruCevap == "b") dogruIndex = 1;
        else if (gecerliDogruCevap == "c") dogruIndex = 2;
        else if (gecerliDogruCevap == "d") dogruIndex = 3;

        List<int> yanlisSiklar = new List<int>();
        for (int i = 0; i < 4; i++) { if (i != dogruIndex) yanlisSiklar.Add(i); }

        yanlisSiklar.RemoveAt(Random.Range(0, yanlisSiklar.Count));

        foreach (int index in yanlisSiklar)
        {
            SecenekTexts[index].text = "";
            SecenekBtns[index].interactable = false;
        }

        SoruText.text = aktifSoruMetni + "\n\n<color=yellow>(%50 Jokeri kullanýldý, 2 yanlýþ þýk elendi!)</color>";
    }

    public void JokerCiftCevapKullan()
    {
        if (ciftCevapKullanildi) return;
        ciftCevapKullanildi = true;
        ciftcevapBtn.interactable = false;

        if (AudioManager.Instance != null) AudioManager.Instance.JokerSesiCal();

        ciftCevapHakkiAktif = true;
        SoruText.text = aktifSoruMetni + "\n\n<color=yellow>ÇÝFT CEVAP AKTÝF! 2 Tahmin hakkýn var, bir þýk seç.</color>";
    }

    public void JokerSeyirciKullan()
    {
        if (seyirciKullanildi) return;
        seyirciKullanildi = true;
        seyirciBtn.interactable = false;

        if (AudioManager.Instance != null) AudioManager.Instance.JokerSesiCal();

        string tavsiye = gecerliDogruCevap.ToUpper();
        int sans = Random.Range(1, 101);
        if (sans > 85)
        {
            string[] tumSiklar = { "A", "B", "C", "D" };
            do { tavsiye = tumSiklar[Random.Range(0, 4)]; }
            while (tavsiye.ToLower() == gecerliDogruCevap);
        }

        SoruText.text = aktifSoruMetni + $"\n\n<color=yellow>(Seyircilerin %81'i '{tavsiye}' þýkkýný seçti!)</color>";
    }

    // --- YENÝ EKLENEN OYUN SONU FONKSÝYONLARI ---
    public void OyunBittiPaneliniAc()
    {
        if (KazandiPaneli != null) KazandiPaneli.SetActive(true);

        if (soruSirasi > 15)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.DogruSesiCal();
            // BURAYA ÝLERÝDE MULTIPLAYER SKOR KODLARI GELECEK
            if (SiralamaText != null) SiralamaText.text = "TEBRÝKLER! 15 SORUYU TAMAMLADIN!\n(Sýralama Multiplayer eklenince buraya gelecek)";
        }
        else
        {
            if (SiralamaText != null) SiralamaText.text = "<color=red>ELENDÝN!</color>\nDurum: DNF (Bitiremedi)";
        }
    }

    public void AnaMenuyeGit()
    {
        SceneManager.LoadScene("menu");
    }

    public void OyundanCik()
    {
        Debug.Log("Oyundan çýkýldý.");
        Application.Quit();
    }

    public void LobiyeGit()
    {
        SceneManager.LoadScene("menu");
    }
}