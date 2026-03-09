using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using TMPro;
using UnityEngine.UI;
using Unity.Collections;

// PARRELSYNC ÝÇÝN ÝSÝM HAFIZASI (Pencereler arasý asla karýþmaz)
public static class OyuncuBilgisi { public static string Ad = "Oyuncu"; }

public class RelayManager : NetworkBehaviour
{
    [Header("1. Aþama: Baðlantý Arayüzü")]
    public GameObject BaglantiPaneli;
    public TMP_InputField KullaniciAdiGirisAlani;
    public TMP_InputField SifreGirisAlani;
    public TextMeshProUGUI KullaniciAdiUyariYazisi;
    public TextMeshProUGUI DurumYazisi;
    public Button OdaKurButonu;
    public Button KatilButonu;

    [Header("2. Aþama: Lobi Arayüzü")]
    public GameObject LobiPaneli;
    public TextMeshProUGUI LobiDurumYazisi;
    public TextMeshProUGUI OyuncuListesiYazisi;
    public Button OyunuBaslatButonu;

    public NetworkVariable<FixedString512Bytes> OrtakOyuncuListesi = new NetworkVariable<FixedString512Bytes>(
        "", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    async void Start()
    {
        if (OdaKurButonu == null || KatilButonu == null || DurumYazisi == null) return;

        OdaKurButonu.interactable = false;
        KatilButonu.interactable = false;

        if (BaglantiPaneli != null) BaglantiPaneli.SetActive(true);
        if (LobiPaneli != null) LobiPaneli.SetActive(false);
        if (KullaniciAdiUyariYazisi != null) KullaniciAdiUyariYazisi.text = "";

        DurumYazisi.text = "Unity Sunucularina Baglaniliyor...";

        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                InitializationOptions options = new InitializationOptions();
                options.SetProfile("Oyuncu_" + Random.Range(10000, 99999));
                await UnityServices.InitializeAsync(options);
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            DurumYazisi.text = "Baglanti Hazir! Kullanici adini gir ve oyuna basla.";
            OdaKurButonu.interactable = true;
            KatilButonu.interactable = true;
        }
        catch (System.Exception e)
        {
            DurumYazisi.text = "<color=red>Bulut Hatasý! Ýnternetinizi kontrol edin.</color>";
            Debug.LogError("BULUT SÝSTEMÝ BAÞLATILAMADI: " + e);
        }
    }

    public override void OnNetworkSpawn()
    {
        OrtakOyuncuListesi.OnValueChanged += (eskiDeger, yeniDeger) => {
            if (OyuncuListesiYazisi != null) OyuncuListesiYazisi.text = yeniDeger.ToString();
        };

        if (IsServer)
        {
            string isim = KullaniciAdiGirisAlani != null ? KullaniciAdiGirisAlani.text : "Kurucu";
            OrtakOyuncuListesi.Value = "BAGLANAN OYUNCULAR:\n- " + isim;
            if (OyuncuListesiYazisi != null) OyuncuListesiYazisi.text = OrtakOyuncuListesi.Value.ToString();
        }
        else if (IsClient)
        {
            string isim = KullaniciAdiGirisAlani != null ? KullaniciAdiGirisAlani.text : "Oyuncu";
            IsimBildirServerRpc(isim);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void IsimBildirServerRpc(string yeniOyuncuIsmi)
    {
        OrtakOyuncuListesi.Value = OrtakOyuncuListesi.Value.ToString() + "\n- " + yeniOyuncuIsmi;
    }

    public async void OdaKur()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized) return;

        if (KullaniciAdiUyariYazisi != null) KullaniciAdiUyariYazisi.text = "";
        string safKullaniciAdi = KullaniciAdiGirisAlani != null ? KullaniciAdiGirisAlani.text.Replace("\u200B", "").Trim() : "Kurucu";

        if (string.IsNullOrEmpty(safKullaniciAdi))
        {
            if (KullaniciAdiUyariYazisi != null) KullaniciAdiUyariYazisi.text = "<color=red>Lütfen bir kullanýcý adý girin!</color>";
            return;
        }

        // ÝSMÝ RAM'E KAYDET (GameManager buradan çekecek)
        OyuncuBilgisi.Ad = safKullaniciAdi;

        if (OdaKurButonu != null) OdaKurButonu.interactable = false;
        if (DurumYazisi != null) DurumYazisi.text = "Oda Kuruluyor...";

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

            NetworkManager.Singleton.NetworkConfig.NetworkTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            NetworkManager.Singleton.StartHost();

            LobiyeGecisYap("Oda Kuruldu! Þifreniz: <color=yellow>" + joinCode + "</color>", true);
        }
        catch (RelayServiceException e)
        {
            if (DurumYazisi != null) DurumYazisi.text = "Hata: Oda Kurulamadý! " + e.Message;
            if (OdaKurButonu != null) OdaKurButonu.interactable = true;
        }
    }

    public async void OdayaKatil()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized) return;

        if (KullaniciAdiUyariYazisi != null) KullaniciAdiUyariYazisi.text = "";
        string safKullaniciAdi = KullaniciAdiGirisAlani != null ? KullaniciAdiGirisAlani.text.Replace("\u200B", "").Trim() : "Oyuncu";

        if (string.IsNullOrEmpty(safKullaniciAdi))
        {
            if (KullaniciAdiUyariYazisi != null) KullaniciAdiUyariYazisi.text = "<color=red>Lütfen bir kullanýcý adý girin!</color>";
            return;
        }

        if (SifreGirisAlani == null || string.IsNullOrWhiteSpace(SifreGirisAlani.text))
        {
            if (DurumYazisi != null) DurumYazisi.text = "<color=red>Hata: Lütfen oda þifresini girin!</color>";
            return;
        }

        // ÝSMÝ RAM'E KAYDET
        OyuncuBilgisi.Ad = safKullaniciAdi;

        if (OdaKurButonu != null) OdaKurButonu.interactable = false;
        if (KatilButonu != null) KatilButonu.interactable = false;
        if (DurumYazisi != null) DurumYazisi.text = "Odaya Baðlanýlýyor...";

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(SifreGirisAlani.text);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes, joinAllocation.Key, joinAllocation.ConnectionData, joinAllocation.HostConnectionData);

            NetworkManager.Singleton.NetworkConfig.NetworkTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            NetworkManager.Singleton.StartClient();

            LobiyeGecisYap("<color=green>Odaya Baþarýyla Katýldýn!</color>\nKurucu bekleniyor...", false);
        }
        catch (RelayServiceException)
        {
            if (DurumYazisi != null) DurumYazisi.text = "<color=red>Hata: Yanlýþ Þifre veya Oda Dolu!</color>";
            if (OdaKurButonu != null) OdaKurButonu.interactable = true;
            if (KatilButonu != null) KatilButonu.interactable = true;
        }
    }

    private void LobiyeGecisYap(string lobiMesaji, bool kurucuMu)
    {
        if (BaglantiPaneli != null) BaglantiPaneli.SetActive(false);
        if (LobiPaneli != null) LobiPaneli.SetActive(true);
        if (LobiDurumYazisi != null) LobiDurumYazisi.text = lobiMesaji;
        if (OyunuBaslatButonu != null) OyunuBaslatButonu.gameObject.SetActive(kurucuMu);
    }

    public void OyunuBaslat()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}