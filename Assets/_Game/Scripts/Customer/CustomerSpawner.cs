using UnityEngine;
using System;

/// <summary>
/// Müşteri spawn ve yönetim sistemi
/// </summary>
public class CustomerSpawner : MonoBehaviour
{
    public static CustomerSpawner Instance;

    [Header("Spawn Ayarları")]
    public Transform spawnPoint;
    public CustomerData[] availableCustomers;
    
    [Header("Mevcut Müşteri")]
    public CustomerData currentCustomerData;
    public GameObject currentCustomerInstance;
    public Animator currentAnimator;

    // Eventler
    public event Action<CustomerData> OnCustomerSpawned;
    public event Action OnCustomerDefeated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        SpawnCustomerByIndex(0); // İlk müşteriyi spawn et
        GameManager.Instance.OnCustomerSpawned(currentCustomerData);
    }
    /// <summary>
    /// Belirli bir müşteri tipini spawn eder
    /// </summary>
    public void SpawnCustomer(CustomerData customerData)
    {
        // Önceki müşteriyi temizle
        if (currentCustomerInstance != null)
        {
            Destroy(currentCustomerInstance);
        }

        currentCustomerData = customerData;

        // Yeni müşteriyi oluştur
        if (customerData.modelPrefab != null && spawnPoint != null)
        {
            currentCustomerInstance = Instantiate(customerData.modelPrefab, spawnPoint.position, spawnPoint.rotation);
            currentCustomerInstance.transform.SetParent(spawnPoint);
            currentCustomerInstance.transform.localPosition = new(0, 0, 0);
            currentAnimator = currentCustomerInstance.GetComponent<Animator>();

            // Animator controller varsa ata
            if (currentAnimator != null && customerData.animatorController != null)
            {
                currentAnimator.runtimeAnimatorController = customerData.animatorController;
            }
        }

        Debug.Log($"[CustomerSpawner] {customerData.customerName} spawn edildi! HP: {customerData.maxHP}");
        OnCustomerSpawned?.Invoke(customerData);
    }

    /// <summary>
    /// Rastgele müşteri spawn eder
    /// </summary>
    public void SpawnRandomCustomer()
    {
        if (availableCustomers != null && availableCustomers.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableCustomers.Length);
            SpawnCustomer(availableCustomers[randomIndex]);
        }
        else
        {
            Debug.LogWarning("[CustomerSpawner] Spawn edilecek müşteri yok!");
        }
    }

    /// <summary>
    /// İndekse göre müşteri spawn eder (zorluk sırası için)
    /// </summary>
    public void SpawnCustomerByIndex(int index)
    {
        if (availableCustomers != null && index >= 0 && index < availableCustomers.Length)
        {
            SpawnCustomer(availableCustomers[index]);
        }
    }

    /// <summary>
    /// Müşteri tipine göre spawn eder
    /// </summary>
    public void SpawnCustomerByType(CustomerType type)
    {
        foreach (var customer in availableCustomers)
        {
            if (customer.customerType == type)
            {
                SpawnCustomer(customer);
                return;
            }
        }
        Debug.LogWarning($"[CustomerSpawner] {type} tipinde müşteri bulunamadı!");
    }

    /// <summary>
    /// Animasyon tetikler
    /// </summary>
    public void PlayAnimation(string triggerName)
    {
        if (currentAnimator != null)
        {
            currentAnimator.SetTrigger(triggerName);
        }
    }

    /// <summary>
    /// Müşteri yenildiğinde çağrılır
    /// </summary>
    public void OnDefeat()
    {
        PlayAnimation("Defeat");
        OnCustomerDefeated?.Invoke();
    }
}
