using UnityEngine;

using System.Collections;

using System.Collections.Generic;

using TMPro;

using UnityEngine.InputSystem;

public class RouletteWheel : MonoBehaviour

{

    public Rigidbody2D wheelBody;

    public Transform pointer;

    public RectTransform slotParent;

    public GameObject slotPrefab;

    public TextMeshProUGUI rollsLeftText;

    public TextMeshProUGUI resultsHistoryText;

    public TextMeshProUGUI reboundsLeftText;

    public GameObject rouletteWheelParent;

    public GameObject enemyPrefab;

    public Transform[] enemySpawnPoints;

    public CombatManager combatManager;

    public TextMeshProUGUI playerHpText;

    public float minTorque = 400f;

    public float maxTorque = 700f;

    public int rollsLeft = 3;

    public int reboundsLeft = 1;

    private bool isSpinning = false;

    private Dictionary<string, int> rollCount = new Dictionary<string, int>();

    private List<SliceData> collectedActions = new List<SliceData>();

    private List<Enemy> activeEnemies = new List<Enemy>();

    private int playerCurrentHp;

    private int playerMaxHp = 30;

    private int playerArmor = 0;

    public List<SliceData> rouletteSlices;

    void Start()

    {

        if (rouletteSlices == null || rouletteSlices.Count == 0)

        {

            Debug.LogError("Roulette Slices not assigned. Please assign them in the inspector.");

            return;

        }

        if (playerHpText == null)

        {

            Debug.LogError("Player Hp Text (TextMeshProUGUI) is not assigned! Please assign it in the inspector.");

        }

        if (rollsLeftText == null)

        {

            Debug.LogError("Rolls Left Text (TextMeshProUGUI) is not assigned! Please assign it in the inspector.");

        }

        if (resultsHistoryText == null)

        {

            Debug.LogError("Results History Text (TextMeshProUGUI) is not assigned! Please assign it in the inspector.");

        }

        if (reboundsLeftText == null)

        {

            Debug.LogError("Rebounds Left Text (TextMeshProUGUI) is not assigned! Please assign it in the inspector.");

        }

        if (rouletteWheelParent == null)

        {

            Debug.LogError("Roulette Wheel Parent is not assigned! Please assign it in the inspector.");

        }

        if (combatManager == null)

        {

            Debug.LogError("Combat Manager is not assigned! Please assign it in the inspector.");

        }

        StartCoroutine(DelayedSetup());

    }

    private IEnumerator DelayedSetup()

    {

        yield return null;

        playerCurrentHp = playerMaxHp;

                playerArmor = 0;
 
         BuildRoulette(rouletteSlices);
 
         UpdatePlayerStats(playerCurrentHp, Mathf.Clamp(playerArmor, 0, 999));

        UpdateRollsLeftUI();

        UpdateRollHistoryUI();

        UpdateReboundsLeftUI();

    }

    private void UpdatePlayerHpDisplay()

    {

        if (playerHpText == null) return;

        string hpString;

        if (playerArmor > 0)

        {

            hpString = $"{playerCurrentHp}+{playerArmor}/{playerMaxHp}";

        }

        else

        {

            hpString = $"{playerCurrentHp}/{playerMaxHp}";

        }

        playerHpText.text = hpString;

    }

        public void UpdatePlayerStats(int hp, int armor)
 
    {
 
        playerCurrentHp = Mathf.Clamp(hp, 0, playerMaxHp);
 
        playerArmor = Mathf.Clamp(armor, 0, 999);
 
        UpdatePlayerHpDisplay();
 
    }

    void Update()

    {

        if (Keyboard.current != null)

        {

            if (Keyboard.current.spaceKey.wasPressedThisFrame)

            {

                Spin();

            }

            if (Keyboard.current.rKey.wasPressedThisFrame)

            {

                Rebound();

            }

        }

    }

    public void Spin()

    {

        if (isSpinning) return;

        if (rollsLeft <= 0)

        {

            Debug.Log("No rolls left!");

            return;

        }

        isSpinning = true;

        rollsLeft--;

        UpdateRollsLeftUI();

        if (wheelBody == null)

        {

            Debug.LogError("WheelBody not assigned.");

            isSpinning = false;

            return;

        }

        wheelBody.angularVelocity = 0;

        float torque = Random.Range(minTorque, maxTorque);

        float jitter = torque * Random.Range(-0.01f, 0.01f);

        float finalTorque = torque + jitter;

        wheelBody.AddTorque(finalTorque, ForceMode2D.Impulse);

        Debug.Log($"Spinning wheel with torque: {finalTorque}");

        StartCoroutine(StopCheck());

        if (rollsLeft <= 0)

        {

            StartCoroutine(WaitForFinalEvaluationAndStartCombat());

        }

    }

    private void UpdateRollsLeftUI()

    {

        if (rollsLeftText != null)

        {

            rollsLeftText.text = $"Rolls Left: {rollsLeft}";

        }

    }

    private void UpdateRollHistoryUI()

    {

        if (resultsHistoryText != null)

        {

            string historyString = "";

            foreach (var result in rollCount)

            {

                if (result.Value > 1)

                {

                    historyString += $"{result.Key}*{result.Value}\n";

                }

                else

                {

                    historyString += $"{result.Key}\n";

                }

            }

            resultsHistoryText.text = historyString;

        }

    }

    private void UpdateReboundsLeftUI()

    {

        if (reboundsLeftText != null)

        {

            reboundsLeftText.text = $"Rebounds Left: {reboundsLeft}";

        }

    }

    public void BuildRoulette(List<SliceData> slices)

    {

        foreach (Transform c in slotParent)

        {

            Destroy(c.gameObject);

        }

        List<SliceData> weighted = new List<SliceData>();

        foreach (var s in slices)

        {

            for (int i = 0; i < s.weight; i++)

            {

                weighted.Add(s);

            }

        }

        if (weighted.Count == 0)

        {

            Debug.LogWarning("No weighted slices to build.");

            return;

        }

        float sliceAngle = 360f / weighted.Count;

        for (int i = 0; i < weighted.Count; i++)

        {

            GameObject go = Instantiate(slotPrefab);

            go.transform.SetParent(slotParent, false);

            RectTransform rt = go.GetComponent<RectTransform>();

            if (rt == null)

            {

                Debug.LogError("Slot prefab is missing RectTransform.");

                Destroy(go);

                continue;

            }

            rt.localPosition = Vector3.zero;

            rt.localScale = Vector3.one;

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, slotParent.rect.width);

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, slotParent.rect.height);

            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);

            rt.localRotation = Quaternion.Euler(0, 0, (i * sliceAngle) + 90f);

            RouletteSlot slot = go.GetComponent<RouletteSlot>();

            if (slot != null)

            {

                slot.Setup(weighted[i], sliceAngle);

            }

            else

            {

                Debug.LogError($"RouletteSlot missing on instantiated slot for {weighted[i].sliceName}.");

            }

        }

    }

    private void StartCombat()

    {

        if (rouletteWheelParent != null)

        {

            rouletteWheelParent.SetActive(false);

        }
        else
        {

            Debug.LogError("Roulette Wheel Parent GameObject not assigned!");

        }

        if (activeEnemies.Count == 0)

        {

            Debug.Log("RouletteWheel: No existing living enemies. Spawning new ones for combat.");

            if (enemyPrefab != null && enemySpawnPoints != null && enemySpawnPoints.Length > 0)

            {

                for (int i = 0; i < enemySpawnPoints.Length; i++)

                {

                    GameObject enemyGo = Instantiate(enemyPrefab, enemySpawnPoints[i].position, Quaternion.identity);

                    Enemy enemyComponent = enemyGo.GetComponent<Enemy>();

                    if (enemyComponent != null)

                    {

                        activeEnemies.Add(enemyComponent);

                        enemyComponent.gameObject.SetActive(true);

                        Debug.Log($"RouletteWheel: Spawned new enemy: {enemyComponent.name}");

                    }

                }

            }

            else

            {

                Debug.LogWarning("Enemy prefab or spawn points are not assigned. Enemies not spawned.");

            }

        }

        else

        {

            Debug.Log($"RouletteWheel: Reactivating {activeEnemies.Count} living enemies for combat.");

            foreach (var enemy in activeEnemies)

            {

                if (enemy != null)

                {

                    enemy.gameObject.SetActive(true);

                    Debug.Log($"RouletteWheel: Reactivated existing enemy: {enemy.name}. Active state: {enemy.gameObject.activeSelf}");

                }

            }

        }

        if (combatManager != null)

        {

            combatManager.InitializeCombat(collectedActions, activeEnemies, playerCurrentHp, playerArmor);

        }

        else

        {

            Debug.LogError("CombatManager reference not set on RouletteWheel.");

        }

        Debug.Log("Combat phase started!");

    }

    public void EndCombat(int finalHp, int finalArmor, List<Enemy> currentLivingEnemies)

    {

        Debug.Log("RouletteWheel: Ending combat (player defeated - full reset).");

                playerCurrentHp = Mathf.Clamp(finalHp, 0, playerMaxHp);
 
        playerArmor = Mathf.Clamp(finalArmor, 0, 999);
 
         rollsLeft = 3;

        reboundsLeft = 1;

        collectedActions.Clear();

        rollCount.Clear();

        Debug.Log($"RouletteWheel: Destroying all {currentLivingEnemies.Count} remaining enemies after player defeat.");

        foreach (Enemy enemy in currentLivingEnemies)

        {

            if (enemy != null)

            {

                Destroy(enemy.gameObject);

            }

        }

        activeEnemies.Clear();

        if (combatManager != null && combatManager.combatUI != null)

        {

            combatManager.combatUI.SetActive(false);

        }

        UpdatePlayerStats(playerCurrentHp, playerArmor);

        UpdateRollsLeftUI();

        UpdateRollHistoryUI();

        UpdateReboundsLeftUI();

        if (rouletteWheelParent != null)

        {

            rouletteWheelParent.SetActive(true);

        }

    }

    public void CombatFullyConcluded(int finalHp)
    {
        Debug.Log("RouletteWheel: All enemies defeated. Combat fully concluded.");
        playerCurrentHp = Mathf.Clamp(finalHp, 0, playerMaxHp);
        playerArmor = 0;
        reboundsLeft = 1;
        rollsLeft = 3;
        collectedActions.Clear();
        rollCount.Clear();
        activeEnemies.Clear();

        UpdatePlayerStats(playerCurrentHp, playerArmor);
        UpdateRollsLeftUI();
        UpdateRollHistoryUI();
        UpdateReboundsLeftUI();

        if (rouletteWheelParent != null)
        {
            rouletteWheelParent.SetActive(true);
        }

    }

    public List<SliceData> GetCurrentPlayerSlices()
    {

        return new List<SliceData>(rouletteSlices);
    }

    public void StartNewRollPhase(int finalHp, int finalArmor, List<Enemy> currentLivingEnemies)

    {

        Debug.Log("Starting new roll phase in RouletteWheel.");

        playerCurrentHp = finalHp;

                playerArmor = Mathf.Clamp(finalArmor, 0, 999);
 
         activeEnemies = new List<Enemy>(currentLivingEnemies);

        Debug.Log($"RouletteWheel: Received {activeEnemies.Count} living enemies for new roll phase (currently hidden).");

        foreach (var enemy in activeEnemies)

        {

            if (enemy != null)

            {

                Debug.Log($"RouletteWheel: Enemy in 'currentLivingEnemies' list: {enemy.name} (HP: {enemy.currentHp}, Active: {enemy.gameObject.activeSelf})");

            }

        }

        rollsLeft = 3;

        collectedActions.Clear();

        rollCount.Clear();

        UpdatePlayerStats(playerCurrentHp, playerArmor);

        UpdateRollsLeftUI();

        UpdateRollHistoryUI();

        if (rouletteWheelParent != null)

        {

            rouletteWheelParent.SetActive(true);

        }
        else
        {

            Debug.LogError("Roulette Wheel Parent GameObject not assigned!");

        }

    }

    private IEnumerator WaitForFinalEvaluationAndStartCombat()

    {

        yield return new WaitUntil(() => !isSpinning);

        StartCombat();

    }

    private IEnumerator StopCheck()

    {

        yield return new WaitForSeconds(1f);

        while (Mathf.Abs(wheelBody.angularVelocity) > 1f)

        {

            yield return null;

        }

        wheelBody.angularVelocity = 0;

        isSpinning = false;

        Evaluate();

    }

    void Evaluate()

    {

        if (pointer == null)

        {

            Debug.LogError("Pointer not assigned.");

            return;

        }

        int totalWeightedSlices = 0;

        if (rouletteSlices == null || rouletteSlices.Count == 0)

        {

            Debug.LogWarning("No slices configured for evaluation.");

            return;

        }

        foreach (var s in rouletteSlices)

        {

            for (int i = 0; i < s.weight; i++)

            {

                totalWeightedSlices++;

            }

        }

        if (totalWeightedSlices == 0)

        {

            Debug.LogWarning("No weighted slices to evaluate.");

            return;

        }

        float sliceAngle = 360f / totalWeightedSlices;

        float currentWheelRotation = wheelBody.transform.eulerAngles.z;

        currentWheelRotation = (currentWheelRotation % 360 + 360) % 360;

        float adjustedPointerWorldAngle = 90f + sliceAngle;

        float angleOnWheelAtPointer = (adjustedPointerWorldAngle - currentWheelRotation + 360f) % 360f;

        float accumulatedAngle = 0f;

        SliceData landedSliceData = null;

        foreach (var s in rouletteSlices)

        {

            for (int i = 0; i < s.weight; i++)

            {

                float sliceStartAngle = accumulatedAngle;

                float sliceEndAngle = accumulatedAngle + sliceAngle;

                if (angleOnWheelAtPointer >= sliceStartAngle && angleOnWheelAtPointer < sliceEndAngle)

                {

                    landedSliceData = s;

                    break;

                }

                accumulatedAngle += sliceAngle;

            }

            if (landedSliceData != null) break;

        }

        if (landedSliceData != null)

        {

            Debug.Log($"Landed on: {landedSliceData.sliceName}");

            if (rollCount.ContainsKey(landedSliceData.sliceName))

            {

                rollCount[landedSliceData.sliceName]++;

            }

            else

            {

                rollCount.Add(landedSliceData.sliceName, 1);

            }

            UpdateRollHistoryUI();

            collectedActions.Add(landedSliceData);

        }

        else

        {

            Debug.LogWarning("Evaluation: No slice matched. This might indicate an issue with angle calculation or pointer position.");

        }

    }
    public void AddSlice(SliceData sliceToAdd)
    {
        if (sliceToAdd != null)
        {
            rouletteSlices.Add(sliceToAdd);
            Debug.Log($"RouletteWheel: Added slice: {sliceToAdd.sliceName}. Total slices now: {rouletteSlices.Count}");

            BuildRoulette(rouletteSlices);
        }
        else
        {
            Debug.LogWarning("RouletteWheel: Attempted to add a null slice.");
        }
    }

    public void RemoveSlice(SliceData sliceToRemove)
    {
        if (rouletteSlices.Contains(sliceToRemove))
        {
            rouletteSlices.Remove(sliceToRemove);
            Debug.Log($"RouletteWheel: Removed slice: {sliceToRemove.sliceName}. Remaining slices: {rouletteSlices.Count}");
            BuildRoulette(rouletteSlices);
        }
        else
        {
            Debug.LogWarning($"RouletteWheel: Attempted to remove slice '{sliceToRemove.sliceName}' but it was not found in the current list.");
        }
    }

    public void Rebound()

    {

        if (isSpinning) return;

        if (reboundsLeft <= 0)

        {

            Debug.Log("No rebounds left!");

            return;

        }

        reboundsLeft--;

        rollsLeft = 3;

        rollCount.Clear();

        collectedActions.Clear();
        Debug.Log($"RouletteWheel: Rebounding. Destroying all {activeEnemies.Count} enemies.");

        foreach (var enemy in activeEnemies)

        {

            if (enemy != null)

            {

                Destroy(enemy.gameObject);

            }

        }

        activeEnemies.Clear();

        UpdatePlayerStats(playerCurrentHp, playerArmor);

        Debug.Log($"Rebounded! Rebounds left: {reboundsLeft}");

        UpdateRollsLeftUI();

        UpdateRollHistoryUI();

        UpdateReboundsLeftUI();

        if (rouletteWheelParent != null && !rouletteWheelParent.activeSelf)

        {

            rouletteWheelParent.SetActive(true);

            if (combatManager != null && combatManager.combatUI != null)

            {

                combatManager.combatUI.SetActive(false);

            }

        }

    }

}