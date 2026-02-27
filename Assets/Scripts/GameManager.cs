using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

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

    // GROQ ÞÝFRENÝ BURAYA YAPIÞTIR!
    private string apiKey = "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";

    private string gecerliDogruCevap = "";

    // --- YENÝ: KAÇINCI SORUDA OLDUÐUMUZU TUTAN DEÐÝÞKEN ---
    private int soruSirasi = 1;

    private string[] kategoriler = { "Tarih", "Coðrafya", "Uzay Bilimi", "Sinema", "Spor", "Müzik", "Teknoloji", "Biyoloji", "Sanat Tarihi", "Edebiyat" };

    void Start()
    {
        ButonlariHazirla();
        soruSirasi = 1; // Oyun baþlarken soruyu 1 yapýyoruz
        YeniSoruGetir();
    }

    public void YeniSoruGetir()
    {
        // Eðer 10 soruyu da geçtiysek oyunu kazandýk demektir!
        if (soruSirasi > 10)
        {
            SoruText.text = "?? ÝNANILMAZ! 10 SORUYU DA BÝLDÝN VE KAZANDIN! ??";
            foreach (Button btn in SecenekBtns) btn.interactable = false;
            return; // Kodun aþaðýya devam etmesini engelliyoruz
        }

        SoruText.text = soruSirasi + ". Soru için Groq Yapay Zeka düþünülüyor...";
        foreach (Button btn in SecenekBtns) btn.interactable = false;

        // Butonlarýn yazýlarýný da temizleyelim ki güzel görünsün
        foreach (TextMeshProUGUI txt in SecenekTexts) txt.text = "...";

        StartCoroutine(YapayZekadanSoruCek());
    }

    IEnumerator YapayZekadanSoruCek()
    {
        // --- YENÝ: ZORLUK DERECESÝNÝ BELÝRLEME ---
        string zorlukDerecesi = "kolay";
        if (soruSirasi >= 4 && soruSirasi <= 7)
        {
            zorlukDerecesi = "orta";
        }
        else if (soruSirasi >= 8)
        {
            zorlukDerecesi = "çok zor";
        }

        string rastgeleKategori = kategoriler[Random.Range(0, kategoriler.Length)];

        // Promptumuzu soru sýrasýna ve zorluða göre güncelledik
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
                    string yapayZekaMetni = response.choices[0].message.content;
                    yapayZekaMetni = yapayZekaMetni.Replace("```json", "").Replace("```", "").Trim();

                    SoruVerisi yeniSoru = JsonUtility.FromJson<SoruVerisi>(yapayZekaMetni);

                    gecerliDogruCevap = yeniSoru.dogruCevap.ToLower().Trim();
                    if (gecerliDogruCevap.Length > 1)
                    {
                        gecerliDogruCevap = gecerliDogruCevap.Substring(0, 1);
                    }

                    Debug.Log("--- " + soruSirasi + ". SORU GELDÝ ---");
                    Debug.Log("Zorluk: " + zorlukDerecesi + " | Kategori: " + rastgeleKategori + " | Doðru Cevap: [" + gecerliDogruCevap + "]");

                    // Ekrana kaçýncý soruda olduðunu yazdýrýyoruz
                    SoruText.text = "SORU " + soruSirasi + ":\n" + yeniSoru.soru;
                    SecenekTexts[0].text = "A) " + yeniSoru.a;
                    SecenekTexts[1].text = "B) " + yeniSoru.b;
                    SecenekTexts[2].text = "C) " + yeniSoru.c;
                    SecenekTexts[3].text = "D) " + yeniSoru.d;

                    foreach (Button btn in SecenekBtns) btn.interactable = true;
                }
            }
            catch (System.Exception e)
            {
                SoruText.text = "Soru geldi ama okunamadý.";
            }
        }
    }

    void ButonlariHazirla()
    {
        SecenekBtns[0].onClick.AddListener(() => CevapKontrol("a"));
        SecenekBtns[1].onClick.AddListener(() => CevapKontrol("b"));
        SecenekBtns[2].onClick.AddListener(() => CevapKontrol("c"));
        SecenekBtns[3].onClick.AddListener(() => CevapKontrol("d"));
    }

    public void CevapKontrol(string secilenSik)
    {
        if (secilenSik == gecerliDogruCevap)
        {
            // Eðer cevap doðruysa, soru sýrasýný 1 arttýrýyoruz!
            soruSirasi++;
            SoruText.text = "DOÐRU! Sýradaki soru hazýrlanýyor...";
            YeniSoruGetir();
        }
        else
        {
            // Yanlýþ cevap verirse elenir
            SoruText.text = "YANLIÞ CEVAP! Maalesef elendin.";
            foreach (Button btn in SecenekBtns) btn.interactable = false;
        }
    }
}