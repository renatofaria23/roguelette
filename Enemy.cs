using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour
{

    public string enemyName;
    public int maxHp;
    public int armor;
    public int attackDamage;

    [HideInInspector]
    public int currentHp;

    public TextMeshProUGUI enemyStatsText;

    public TextMeshProUGUI enemyActionText;

    public TextMeshProUGUI hpTextToHighlight;
    private Color defaultHpColor;

    private void Awake()
    {

        if (hpTextToHighlight != null)
        {
            defaultHpColor = hpTextToHighlight.color;
        }
        else if (enemyStatsText != null)
        {
            defaultHpColor = enemyStatsText.color;
            hpTextToHighlight = enemyStatsText;
        }
        else
        {
            Debug.LogWarning($"No TextMeshProUGUI component assigned to hpTextToHighlight or enemyStatsText on {gameObject.name}. HP highlighting will not work.");
        }

        Initialize();
    }

    public void Initialize()
    {
        currentHp = maxHp;
        UpdateUI();
    }

    public void TakeDamage(int damageTaken)
    {
        int damageAfterArmor = damageTaken - armor;
        if (damageAfterArmor < 0)
        {
            damageAfterArmor = 0;
        }

        currentHp -= damageAfterArmor;
        if (currentHp < 0)
        {
            currentHp = 0;
        }

        UpdateUI();
        Debug.Log($"Enemy {enemyName} took {damageAfterArmor} damage. Remaining HP: {currentHp}");
    }

    public void SetActionText(string text)
    {
        if (enemyActionText != null)
        {
            enemyActionText.text = text;
        }
        else
        {
            Debug.LogError("Enemy action text is null on " + gameObject.name + ". Cannot set text. Did you link the TextMeshProUGUI component in the inspector?");
        }
    }

    public void UpdateUI()
    {
        if (enemyStatsText != null)
        {

            string hpAndArmorText = armor > 0 ? $"{currentHp}+{armor}/{maxHp}" : $"{currentHp}/{maxHp}";
            enemyStatsText.text = $"{enemyName}\nHP: {hpAndArmorText}";
        }
        else
        {
            Debug.LogError("Enemy stats text is null on " + gameObject.name + ". Cannot update UI. Did you link the TextMeshProUGUI component in the inspector?");
        }
    }

    public void HighlightHP(bool highlight, Color highlightColor)
    {
        if (hpTextToHighlight != null)
        {
            hpTextToHighlight.color = highlight ? highlightColor : defaultHpColor;
        }

    }
}