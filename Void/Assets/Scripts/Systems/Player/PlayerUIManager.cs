using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class PlayerUIManager : NetworkBehaviour
{
    [SerializeField] private Image healthBarPrefab; 
    [SerializeField] private Canvas uiCanvas;    
    [SerializeField] private RectTransform targetPosition; 
    [SerializeField] private RectTransform deathPosition; 
    [SerializeField] private GameObject deathMessagePrefab;
    [SerializeField] private GameObject generalHUD;

    private Image healthBarInstance;
    private DamageSystem damageSystem;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SetupHealthBar();
        }
    }

    private void Awake()
    {
        if (uiCanvas == null)
        {
            GameObject canvasObject = GameObject.Find("Canvas"); 
            if (canvasObject != null)
            {
                uiCanvas = canvasObject.GetComponent<Canvas>();
            }
            else
            {
                Debug.LogError("UICanvas not found in the scene!");
            }
        }

        
        /*if (generalHUD == null)
        {
            GameObject generalHUDObject = GameObject.Find("GeneralHUD");
            if (generalHUDObject != null)
            {
                generalHUD.SetActive(true);
            }
        }*/

        if (targetPosition == null)
        {
            GameObject targetObject = GameObject.Find("HPFrame"); 
            if (targetObject != null)
            {
                targetPosition = targetObject.GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogError("Target UI element not found!");
            }
        }

        if (deathPosition == null)
        {
            GameObject targetObject = GameObject.Find("DeathLoc"); 
            if (targetObject != null)
            {
                deathPosition = targetObject.GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogError("Target UI element not found!");
            }
        }
    }

    private void Start()
    {
        damageSystem = GetComponent<DamageSystem>();
        if (damageSystem == null)
        {
            Debug.LogError("DamageSystem not found on player!");
            return;
        }

        if (IsOwner && healthBarInstance != null)
        {
            damageSystem.OnDamageTaken += UpdateHealthBar;
            damageSystem.OnDeath += DisplayDeathMessage;
        }
    }

    private void SetupHealthBar()
    {
        if (healthBarPrefab == null || uiCanvas == null)
        {
            Debug.LogError("Health bar prefab or UI canvas not assigned!");
            return;
        }

        healthBarInstance = Instantiate(healthBarPrefab, uiCanvas.transform);

        if (targetPosition != null)
        {
            RectTransform healthBarTransform = healthBarInstance.GetComponent<RectTransform>();
            if (healthBarTransform != null)
            {
                healthBarTransform.position = targetPosition.position;
            }
        }

        if (damageSystem != null)
        {
            UpdateHealthBar(damageSystem.currentHealth.Value, damageSystem.totalHealth.Value);
        }
    }

    private void UpdateHealthBar(int currentHealth, int totalHealth)
    {
        if (healthBarInstance != null)
        {
            healthBarInstance.fillAmount = (float)currentHealth / totalHealth;

            if (targetPosition != null)
            {
                RectTransform healthBarTransform = healthBarInstance.GetComponent<RectTransform>();
                if (healthBarTransform != null)
                {
                    healthBarTransform.position = targetPosition.position;
                }
            }
        }
    }

    private void DisplayDeathMessage()
    {
        if (deathMessagePrefab == null || deathPosition == null)
        {
            Debug.LogError("Death message prefab or death position is not assigned!");
            return;
        }

        GameObject deathMessage = Instantiate(deathMessagePrefab, deathPosition.position, Quaternion.identity, uiCanvas.transform);

        Text deathMessageText = deathMessage.GetComponent<Text>();
        if (deathMessageText != null)
        {
            deathMessageText.text = "You Died!";
        }
    }

    private void OnDestroy()
    {
        if (damageSystem != null)
        {
            damageSystem.OnDamageTaken -= UpdateHealthBar;
            damageSystem.OnDeath -= DisplayDeathMessage;
        }

        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance.gameObject);
        }
    }
}
