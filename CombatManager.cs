using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class CombatManager : MonoBehaviour
{
    public RouletteWheel rouletteWheel;
    public GameObject combatUI;
    public GameObject playerActionsPanel;
    public GameObject actionSliceUIPrefab;
    public GameObject postCombatOptionsPanel;
    public TextMeshProUGUI addCardOptionText;
    public TextMeshProUGUI removeCardOptionText;
    public GameObject cardSelectionPanel;
    public GameObject cardSliceUIPrefab;
    public GameObject addCardSelectionPanel;
    public GameObject addCardSliceUIPrefab;
    public List<SliceData> allAvailableSlices;
    private List<TextMeshProUGUI> playerActionUIElements = new List<TextMeshProUGUI>();
    private int selectedActionIndex = 0;
    private Color defaultActionColor;
    private int selectedEnemyIndex = 0;
    public Color selectedEnemyColor = Color.yellow;
    private int selectedPostCombatOptionIndex = 0;
    private List<TextMeshProUGUI> postCombatOptionUIElements = new List<TextMeshProUGUI>();
    private bool awaitingPostCombatChoice = false;
    private List<TextMeshProUGUI> cardSelectionUIElements = new List<TextMeshProUGUI>();
    private int selectedCardRemovalIndex = 0;
    private bool awaitingCardRemovalChoice = false;
    private List<TextMeshProUGUI> addCardChoiceUIElements = new List<TextMeshProUGUI>();
    private List<SliceData> currentAddCardChoices = new List<SliceData>();
    private int selectedAddCardChoiceIndex = 0;
    private bool awaitingAddCardChoice = false;
    private List<Enemy> activeEnemies = new List<Enemy>();
    private List<EnemyAttack> enemyAttacksForThisTurn = new List<EnemyAttack>();
    private List<SliceData> playerActions = new List<SliceData>();
    private int playerCurrentHp;
    private int playerCurrentArmor;
    private int playerMaxHp = 30;
    private const int PlayerMaxArmor = 999;
    private bool combatOver = false;

    void Start()
    {
        if (actionSliceUIPrefab != null && actionSliceUIPrefab.GetComponent<TextMeshProUGUI>() != null)
        {
            defaultActionColor = actionSliceUIPrefab.GetComponent<TextMeshProUGUI>().color;
        }
        else
        {
            Debug.LogError("Action Slice UI Prefab or its TextMeshProUGUI component is missing! Cannot set defaultActionColor.");
            defaultActionColor = Color.white;
        }
        if (postCombatOptionsPanel != null)
        {
            postCombatOptionsPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Post Combat Options Panel is not assigned! Please assign it in the Inspector.");
        }
        if (cardSelectionPanel != null)
        {
            cardSelectionPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Card Selection Panel is not assigned! Please assign it in the Inspector.");
        }
        if (addCardSelectionPanel != null)
        {
            addCardSelectionPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Add Card Selection Panel is not assigned! Please assign it in the Inspector.");
        }
        if (playerActionsPanel == null)
        {
            Debug.LogError("Player Actions Panel is not assigned! Please assign it in the Inspector.");
        }
        if (addCardOptionText != null) postCombatOptionUIElements.Add(addCardOptionText);
        else Debug.LogError("Add Card Option Text is not assigned!");
        if (removeCardOptionText != null) postCombatOptionUIElements.Add(removeCardOptionText);
        else Debug.LogError("Remove Card Option Text is not assigned!");
        UpdatePostCombatOptionHighlight();
        if (allAvailableSlices == null || allAvailableSlices.Count == 0)
        {
            Debug.LogWarning("No SliceData Scriptable Objects assigned to 'allAvailableSlices' in CombatManager! Please assign them in the Inspector.");
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (awaitingPostCombatChoice)
        {
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                selectedPostCombatOptionIndex = (selectedPostCombatOptionIndex - 1 + postCombatOptionUIElements.Count) % postCombatOptionUIElements.Count;
                UpdatePostCombatOptionHighlight();
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                selectedPostCombatOptionIndex = (selectedPostCombatOptionIndex + 1) % postCombatOptionUIElements.Count;
                UpdatePostCombatOptionHighlight();
            }
            else if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ConfirmPostCombatChoice();
            }
        }
        else if (awaitingCardRemovalChoice)
        {
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                selectedCardRemovalIndex = (selectedCardRemovalIndex - 1 + cardSelectionUIElements.Count) % cardSelectionUIElements.Count;
                UpdateCardRemovalHighlight();
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                selectedCardRemovalIndex = (selectedCardRemovalIndex + 1) % cardSelectionUIElements.Count;
                UpdateCardRemovalHighlight();
            }
            else if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ConfirmCardRemovalChoice();
            }
        }
        else if (awaitingAddCardChoice)
        {
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                selectedAddCardChoiceIndex = (selectedAddCardChoiceIndex - 1 + addCardChoiceUIElements.Count) % addCardChoiceUIElements.Count;
                UpdateAddCardHighlight();
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                selectedAddCardChoiceIndex = (selectedAddCardChoiceIndex + 1) % addCardChoiceUIElements.Count;
                UpdateAddCardHighlight();
            }
            else if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ConfirmAddCardChoice();
            }
        }
    }

    public void InitializeCombat(List<SliceData> actions, List<Enemy> enemies, int hp, int armor)
    {
        if (postCombatOptionsPanel != null)
        {
            postCombatOptionsPanel.SetActive(false);
            awaitingPostCombatChoice = false;
        }
        if (cardSelectionPanel != null)
        {
            cardSelectionPanel.SetActive(false);
            awaitingCardRemovalChoice = false;
            CleanUpCardSelectionUI();
        }
        if (addCardSelectionPanel != null)
        {
            addCardSelectionPanel.SetActive(false);
            awaitingAddCardChoice = false;
            CleanUpAddCardSelectionUI();
        }
        if (combatUI != null)
        {
            combatUI.SetActive(true);
        }
        else
        {
            Debug.LogError("Combat UI GameObject not assigned!");
        }
        playerActions = new List<SliceData>(actions);
        activeEnemies = new List<Enemy>(enemies);
        playerCurrentHp = hp;
        playerCurrentArmor = Mathf.Clamp(armor, 0, PlayerMaxArmor);
        combatOver = false;
        if (activeEnemies.Count == 0)
        {
            Debug.LogError("No enemies were passed to the Combat Manager! Combat cannot start.");
            return;
        }
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                enemy.gameObject.SetActive(true);
                enemy.UpdateUI();
                enemy.HighlightHP(false, selectedEnemyColor);
            }
        }
        selectedEnemyIndex = 0;
        if (activeEnemies.Count > 0)
        {
            FindNextLivingEnemy(ref selectedEnemyIndex, true);
            UpdateEnemyHighlight();
        }
        InitializePlayerActionUI();
        Debug.Log("Combat initialized with " + playerActions.Count + " actions and " + activeEnemies.Count + " enemies.");
        PrepareEnemyTurn();
        StartCoroutine(PlayerTurn_Manual());
    }

    private void InitializePlayerActionUI()
    {
        foreach (var uiElement in playerActionUIElements)
        {
            Destroy(uiElement.gameObject);
        }
        playerActionUIElements.Clear();
        if (actionSliceUIPrefab == null)
        {
            Debug.LogError("Action Slice UI Prefab is not assigned!");
            return;
        }
        if (playerActionsPanel == null)
        {
            Debug.LogError("Player Actions Panel is not assigned! Cannot build player action UI.");
            return;
        }
        if (actionSliceUIPrefab.GetComponent<TextMeshProUGUI>() == null)
        {
            Debug.LogError("Action Slice UI Prefab is missing a TextMeshProUGUI component!");
            return;
        }
        for (int i = 0; i < playerActions.Count; i++)
        {
            GameObject uiObject = Instantiate(actionSliceUIPrefab, playerActionsPanel.transform);
            TextMeshProUGUI actionText = uiObject.GetComponent<TextMeshProUGUI>();
            playerActionUIElements.Add(actionText);
            actionText.text = playerActions[i].sliceName;
        }
        selectedActionIndex = 0;
        UpdateActionHighlight();
    }

    private void PrepareEnemyTurn()
    {
        enemyAttacksForThisTurn.Clear();
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null && enemy.currentHp > 0)
            {
                enemyAttacksForThisTurn.Add(new EnemyAttack { enemy = enemy, damage = enemy.attackDamage });
                enemy.SetActionText($"Deals {enemy.attackDamage} damage!");
            }
        }
    }

    private IEnumerator PlayerTurn_Manual()
    {
        Debug.Log("Player's turn has started. Awaiting input...");
        if (playerActionUIElements.Count == 0 && !AnyLivingEnemies())
        {
            Debug.Log("Player has no actions and no enemies. Combat concluded.");
            EvaluateCombatEnd();
            yield break;
        }
        else if (playerActionUIElements.Count == 0)
        {
            Debug.Log("Player has no actions left, enemies are still alive. Transitioning to enemy turn.");
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(EnemyTurn());
            yield break;
        }

        while (true)
        {
            if (combatOver)
            {
                Debug.Log("PlayerTurn_Manual exiting: Combat is already over.");
                yield break;
            }

            if (Keyboard.current != null && Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                if (activeEnemies.Count > 0)
                {
                    UpdateEnemyHighlight(false);
                    selectedEnemyIndex = (selectedEnemyIndex - 1 + activeEnemies.Count) % activeEnemies.Count;
                    FindNextLivingEnemy(ref selectedEnemyIndex, false);
                    UpdateEnemyHighlight(true);
                }
            }
            else if (Keyboard.current != null && Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                if (activeEnemies.Count > 0)
                {
                    UpdateEnemyHighlight(false);
                    selectedEnemyIndex = (selectedEnemyIndex + 1) % activeEnemies.Count;
                    FindNextLivingEnemy(ref selectedEnemyIndex, false);
                    UpdateEnemyHighlight(true);
                }
            }

            if (Keyboard.current != null && Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                selectedActionIndex = (selectedActionIndex + 1) % playerActionUIElements.Count;
                UpdateActionHighlight();
            }
            else if (Keyboard.current != null && Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                selectedActionIndex = (selectedActionIndex - 1 + playerActionUIElements.Count) % playerActionUIElements.Count;
                UpdateActionHighlight();
            }

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (!AnyLivingEnemies())
                {
                    Debug.Log("No living enemies left to target. Exiting player turn.");
                    break;
                }
                SliceData selectedAction = playerActions[selectedActionIndex];
                Enemy targetEnemy = GetValidEnemyAtIndex(selectedEnemyIndex);
                if (targetEnemy == null)
                {
                    Debug.LogWarning("Selected enemy is no longer valid. Retargeting...");
                    FindNextLivingEnemy(ref selectedEnemyIndex, true);
                    targetEnemy = GetValidEnemyAtIndex(selectedEnemyIndex);
                    if (targetEnemy == null)
                    {
                        Debug.LogWarning("No living enemies to target with the action! (Action not consumed)");
                        yield return null;
                        continue;
                    }
                }
                ApplyAction(selectedAction, targetEnemy);
                playerActions.RemoveAt(selectedActionIndex);
                Destroy(playerActionUIElements[selectedActionIndex].gameObject);
                playerActionUIElements.RemoveAt(selectedActionIndex);

                if (playerActionUIElements.Count > 0)
                {
                    if (selectedActionIndex >= playerActionUIElements.Count)
                    {
                        selectedActionIndex = playerActionUIElements.Count - 1;
                    }
                    UpdateActionHighlight();
                }
                else
                {
                    CleanUpPlayerActionUI();
                }

                EvaluateCombatEnd();
                if (combatOver)
                {
                    Debug.Log("PlayerTurn_Manual exiting immediately after EvaluateCombatEnd: Combat concluded.");
                    yield break;
                }

                if (AnyLivingEnemies())
                {
                    FindNextLivingEnemy(ref selectedEnemyIndex, true);
                    UpdateEnemyHighlight(true);
                }
                else
                {
                    selectedEnemyIndex = -1;
                    UpdateEnemyHighlight(false);
                    Debug.Log("All enemies defeated after action. Player turn concluding.");
                    break;
                }

                if (playerActions.Count > 0)
                {
                    yield return null;
                }
                else
                {
                    Debug.Log("All player actions used for this turn. Transitioning to enemy turn.");
                    break;
                }
            }

            yield return null;
        }

        yield return null;
        yield return new WaitForSeconds(1.0f);

        if (!combatOver)
        {
            StartCoroutine(EnemyTurn());
        }
        else
        {
            Debug.Log("Combat is over, not starting enemy turn.");
        }
    }

    private IEnumerator EnemyTurn()
    {
        if (combatOver) yield break;
        Debug.Log("Enemy's turn has started. Player's current armor is: " + playerCurrentArmor);
        foreach (var attack in enemyAttacksForThisTurn)
        {
            if (attack.enemy != null && attack.enemy.currentHp > 0)
            {
                var enemy = attack.enemy;
                int enemyDamage = attack.damage;
                int damageToArmor = Mathf.Min(enemyDamage, playerCurrentArmor);
                playerCurrentArmor = Mathf.Clamp(playerCurrentArmor - damageToArmor, 0, PlayerMaxArmor);
                int damageToHp = enemyDamage - damageToArmor;
                playerCurrentHp -= damageToHp;
                if (playerCurrentHp < 0) playerCurrentHp = 0;
                Debug.Log($"Enemy dealt {enemyDamage} damage. Armor absorbed {damageToArmor}. Player took {damageToHp}. HP: {playerCurrentHp}, Armor: {playerCurrentArmor}");
                rouletteWheel?.UpdatePlayerStats(playerCurrentHp, playerCurrentArmor);
                // Trigger OnAttacked powers
                rouletteWheel?.OnPlayerWasAttacked(enemyDamage, damageToHp, damageToArmor);
                enemy.SetActionText("");
                yield return new WaitForSeconds(1.0f);
            }
        }
        EvaluateCombatEnd();
        if (!combatOver)
        {
            Debug.Log("Enemy turn complete. Starting a new roll phase.");
            EndCombatUI();
            rouletteWheel?.StartNewRollPhase(playerCurrentHp, playerCurrentArmor, activeEnemies);
        }
        else
        {
            Debug.Log("Combat ended after enemy turn (either player lost or enemies died from status effect if implemented, leading to Win state).");
        }
    }

    private void EndCombatUI()
    {
        if (combatUI != null) combatUI.SetActive(false);
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                enemy.HighlightHP(false, selectedEnemyColor);
                enemy.gameObject.SetActive(false);
                enemy.SetActionText("");
            }
        }
    }

    private void ApplyAction(SliceData action, Enemy targetEnemy)
    {
        if (targetEnemy == null) return;
        switch (action.effect)
        {
            case SliceEffectType.Damage:
                int damage = action.power;
                Debug.Log($"Applying {damage} damage to enemy {targetEnemy.name}.");
                targetEnemy.TakeDamage(damage);
                // Trigger OnDealDamage powers
                rouletteWheel?.OnPlayerDealtDamage(damage);
                break;
            case SliceEffectType.Heal:
                playerCurrentHp += action.power;
                if (playerCurrentHp > playerMaxHp) playerCurrentHp = playerMaxHp;
                Debug.Log($"Player healed for {action.power}. New HP: {playerCurrentHp}");
                rouletteWheel?.UpdatePlayerStats(playerCurrentHp, playerCurrentArmor);
                break;
            case SliceEffectType.Armor:
                playerCurrentArmor = Mathf.Clamp(playerCurrentArmor + action.power, 0, PlayerMaxArmor);
                Debug.Log($"Player gained {action.power} armor. New Armor: {playerCurrentArmor}");
                rouletteWheel?.UpdatePlayerStats(playerCurrentHp, playerCurrentArmor);
                break;
            default:
                Debug.LogWarning("Unknown slice effect type: " + action.effect);
                break;
        }
    }

    private void EvaluateCombatEnd()
    {
        Debug.Log("--- Evaluating Combat End ---");
        if (activeEnemies == null)
        {
            Debug.LogWarning("activeEnemies list is null in EvaluateCombatEnd. This should not happen.");
            return;
        }
        List<Enemy> enemiesToDestroy = new List<Enemy>();
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null && enemy.currentHp <= 0)
            {
                enemiesToDestroy.Add(enemy);
            }
        }
        foreach (var enemy in enemiesToDestroy)
        {
            if (enemy != null && enemy.gameObject != null)
            {
                enemy.HighlightHP(false, selectedEnemyColor);
                Destroy(enemy.gameObject);
                Debug.Log($"Enemy {enemy.name} has been destroyed.");
            }
        }
        int initialEnemyCount = activeEnemies.Count;
        activeEnemies.RemoveAll(enemy => enemy == null);
        activeEnemies.RemoveAll(enemy => enemy.currentHp <= 0);
        Debug.Log($"Removed {initialEnemyCount - activeEnemies.Count} defeated enemies from activeEnemies list. Remaining: {activeEnemies.Count}");
        string remainingEnemyNames = "Remaining Enemies in CombatManager (all should be alive): ";
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                remainingEnemyNames += $"{enemy.name} (HP: {enemy.currentHp}, Active: {enemy.gameObject.activeSelf}), ";
            }
        }
        if (activeEnemies.Count == 0)
        {
            remainingEnemyNames += "None.";
        }
        Debug.Log(remainingEnemyNames);
        Debug.Log($"Player HP: {playerCurrentHp}");
        if (activeEnemies.Count == 0)
        {
            combatOver = true;
            Debug.Log("All enemies defeated! Combat fully concluded. Showing post-combat options.");
            EndCombatUI();
            CleanUpPlayerActionUI();
            StartCoroutine(ShowPostCombatOptions());
        }
        else if (playerCurrentHp <= 0)
        {
            combatOver = true;
            Debug.Log("Player defeated! Ending combat (full reset).");
            EndCombatUI();
            CleanUpPlayerActionUI();
            rouletteWheel?.EndCombat(playerCurrentHp, playerCurrentArmor, activeEnemies);
        }
        Debug.Log("--- End Evaluation ---");
    }

    private IEnumerator ShowPostCombatOptions()
    {
        yield return new WaitForSeconds(1.0f);
        if (postCombatOptionsPanel != null)
        {
            postCombatOptionsPanel.SetActive(true);
            selectedPostCombatOptionIndex = 0;
            UpdatePostCombatOptionHighlight();
            awaitingPostCombatChoice = true;
        }
        else
        {
            Debug.LogError("Post Combat Options Panel is not assigned! Cannot show options.");
            rouletteWheel?.CombatFullyConcluded(playerCurrentHp);
        }
    }

    private void ConfirmPostCombatChoice()
    {
        if (!awaitingPostCombatChoice) return;
        awaitingPostCombatChoice = false;
        if (postCombatOptionsPanel != null)
        {
            postCombatOptionsPanel.SetActive(false);
        }
        if (selectedPostCombatOptionIndex == 0)
        {
            Debug.Log("ADD CARD selected. Showing card addition options.");
            StartCoroutine(ShowAddCardSelection());
        }
        else
        {
            Debug.Log("REMOVE CARD selected. Showing list of cards for removal.");
            StartCoroutine(ShowCardRemovalSelection());
        }
    }

    private IEnumerator ShowCardRemovalSelection()
    {
        yield return new WaitForSeconds(0.5f);
        if (cardSelectionPanel == null || cardSliceUIPrefab == null || cardSliceUIPrefab.GetComponent<TextMeshProUGUI>() == null)
        {
            Debug.LogError("Card Selection Panel or Card Slice UI Prefab / TextMeshProUGUI is not assigned! Cannot show card selection.");
            rouletteWheel?.CombatFullyConcluded(playerCurrentHp);
            yield break;
        }
        List<SliceData> currentPlayerSlices = rouletteWheel.GetCurrentPlayerSlices();
        if (currentPlayerSlices.Count == 0)
        {
            Debug.LogWarning("Player has no slices to remove. Skipping card removal.");
            rouletteWheel?.CombatFullyConcluded(playerCurrentHp);
            yield break;
        }
        CleanUpCardSelectionUI();
        foreach (var slice in currentPlayerSlices)
        {
            GameObject uiObject = Instantiate(cardSliceUIPrefab, cardSelectionPanel.transform);
            TextMeshProUGUI sliceText = uiObject.GetComponent<TextMeshProUGUI>();
            cardSelectionUIElements.Add(sliceText);
            sliceText.text = slice.sliceName;
        }
        cardSelectionPanel.SetActive(true);
        selectedCardRemovalIndex = 0;
        UpdateCardRemovalHighlight();
        awaitingCardRemovalChoice = true;
    }

    private void ConfirmCardRemovalChoice()
    {
        if (!awaitingCardRemovalChoice) return;
        if (selectedCardRemovalIndex < 0 || selectedCardRemovalIndex >= cardSelectionUIElements.Count)
        {
            Debug.LogWarning("Invalid card removal selection index.");
            awaitingCardRemovalChoice = false;
            if (cardSelectionPanel != null) cardSelectionPanel.SetActive(false);
            CleanUpCardSelectionUI();
            rouletteWheel?.CombatFullyConcluded(playerCurrentHp);
            return;
        }
        List<SliceData> currentPlayerSlices = rouletteWheel.GetCurrentPlayerSlices();
        if (selectedCardRemovalIndex < currentPlayerSlices.Count)
        {
            SliceData sliceToRemove = currentPlayerSlices[selectedCardRemovalIndex];
            Debug.Log($"Confirmed card removal choice: {sliceToRemove.sliceName}");
            rouletteWheel?.RemoveSlice(sliceToRemove);
        }
        else
        {
            Debug.LogWarning($"Attempted to remove card at index {selectedCardRemovalIndex}, but the corresponding slice was not found in the RouletteWheel's current slices.");
        }
        awaitingCardRemovalChoice = false;
        if (cardSelectionPanel != null)
        {
            cardSelectionPanel.SetActive(false);
        }
        CleanUpCardSelectionUI();
        rouletteWheel?.CombatFullyConcluded(playerCurrentHp);
    }

    private IEnumerator ShowAddCardSelection()
    {
        yield return new WaitForSeconds(0.5f);
        if (addCardSelectionPanel == null || addCardSliceUIPrefab == null || addCardSliceUIPrefab.GetComponent<TextMeshProUGUI>() == null)
        {
            Debug.LogError("Add Card Selection Panel or Add Card Slice UI Prefab / TextMeshProUGUI is not assigned! Cannot show card selection.");
            rouletteWheel?.CombatFullyConcluded(playerCurrentHp);
            yield break;
        }
        if (allAvailableSlices == null || allAvailableSlices.Count == 0)
        {
            Debug.LogWarning("No available slices to add! Skipping card addition.");
            rouletteWheel?.CombatFullyConcluded(playerCurrentHp);
            yield break;
        }
        CleanUpAddCardSelectionUI();
        currentAddCardChoices.Clear();
        List<SliceData> tempAvailableSlices = new List<SliceData>(allAvailableSlices);
        for (int i = 0; i < 3 && tempAvailableSlices.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, tempAvailableSlices.Count);
            currentAddCardChoices.Add(tempAvailableSlices[randomIndex]);
            tempAvailableSlices.RemoveAt(randomIndex);
        }
        foreach (var slice in currentAddCardChoices)
        {
            GameObject uiObject = Instantiate(addCardSliceUIPrefab, addCardSelectionPanel.transform);
            TextMeshProUGUI sliceText = uiObject.GetComponent<TextMeshProUGUI>();
            addCardChoiceUIElements.Add(sliceText);
            sliceText.text = slice.sliceName;
        }
        addCardSelectionPanel.SetActive(true);
        selectedAddCardChoiceIndex = 0;
        UpdateAddCardHighlight();
        awaitingAddCardChoice = true;
    }

    private void ConfirmAddCardChoice()
    {
        if (!awaitingAddCardChoice) return;
        if (selectedAddCardChoiceIndex < 0 || selectedAddCardChoiceIndex >= currentAddCardChoices.Count)
        {
            Debug.LogWarning("Invalid add card selection index.");
            awaitingAddCardChoice = false;
            if (addCardSelectionPanel != null) addCardSelectionPanel.SetActive(false);
            CleanUpAddCardSelectionUI();
            rouletteWheel?.CombatFullyConcluded(playerCurrentHp);
            return;
        }
        SliceData sliceToAdd = currentAddCardChoices[selectedAddCardChoiceIndex];
        Debug.Log($"Confirmed card to add: {sliceToAdd.sliceName}");
        rouletteWheel?.AddSlice(sliceToAdd);
        awaitingAddCardChoice = false;
        if (addCardSelectionPanel != null)
        {
            addCardSelectionPanel.SetActive(false);
        }
        CleanUpAddCardSelectionUI();
        rouletteWheel?.CombatFullyConcluded(playerCurrentHp);
    }

    private void CleanUpPlayerActionUI()
    {
        foreach (var uiElement in playerActionUIElements)
        {
            if (uiElement != null)
            {
                Destroy(uiElement.gameObject);
            }
        }
        playerActionUIElements.Clear();
    }

    private void CleanUpCardSelectionUI()
    {
        foreach (var uiElement in cardSelectionUIElements)
        {
            if (uiElement != null)
            {
                Destroy(uiElement.gameObject);
            }
        }
        cardSelectionUIElements.Clear();
    }

    private void CleanUpAddCardSelectionUI()
    {
        foreach (var uiElement in addCardChoiceUIElements)
        {
            if (uiElement != null)
            {
                Destroy(uiElement.gameObject);
            }
        }
        addCardChoiceUIElements.Clear();
        currentAddCardChoices.Clear();
    }

    private void UpdateActionHighlight()
    {
        if (playerActionUIElements.Count == 0) return;
        for (int i = 0; i < playerActionUIElements.Count; i++)
        {
            playerActionUIElements[i].color = (i == selectedActionIndex) ? Color.yellow : defaultActionColor;
        }
    }

    private void UpdateEnemyHighlight(bool highlightCurrent = true)
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if (activeEnemies[i] != null)
            {
                bool isSelectedAndAlive = (i == selectedEnemyIndex) && highlightCurrent && activeEnemies[i].currentHp > 0;
                activeEnemies[i].HighlightHP(isSelectedAndAlive, selectedEnemyColor);
            }
        }
    }

    private void UpdatePostCombatOptionHighlight()
    {
        if (postCombatOptionUIElements.Count == 0) return;
        for (int i = 0; i < postCombatOptionUIElements.Count; i++)
        {
            if (postCombatOptionUIElements[i] != null)
            {
                postCombatOptionUIElements[i].color = (i == selectedPostCombatOptionIndex) ? Color.yellow : defaultActionColor;
            }
        }
    }

    private void UpdateCardRemovalHighlight()
    {
        if (cardSelectionUIElements.Count == 0) return;
        for (int i = 0; i < cardSelectionUIElements.Count; i++)
        {
            if (cardSelectionUIElements[i] != null)
            {
                cardSelectionUIElements[i].color = (i == selectedCardRemovalIndex) ? Color.yellow : defaultActionColor;
            }
        }
    }

    private void UpdateAddCardHighlight()
    {
        if (addCardChoiceUIElements.Count == 0) return;
        for (int i = 0; i < addCardChoiceUIElements.Count; i++)
        {
            if (addCardChoiceUIElements[i] != null)
            {
                addCardChoiceUIElements[i].color = (i == selectedAddCardChoiceIndex) ? Color.yellow : defaultActionColor;
            }
        }
    }

    private void FindNextLivingEnemy(ref int startIndex, bool wrapAround)
    {
        if (activeEnemies.Count == 0)
        {
            startIndex = -1;
            return;
        }
        int originalStartIndex = startIndex;
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            int currentIndex = (originalStartIndex + i) % activeEnemies.Count;
            if (currentIndex < 0) currentIndex += activeEnemies.Count;
            if (activeEnemies[currentIndex] != null && activeEnemies[currentIndex].currentHp > 0)
            {
                startIndex = currentIndex;
                return;
            }
        }
        startIndex = -1;
    }

    private bool AnyLivingEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null && enemy.currentHp > 0)
                return true;
        }
        return false;
    }

    private Enemy GetValidEnemyAtIndex(int index)
    {
        if (index >= 0 && index < activeEnemies.Count)
        {
            var e = activeEnemies[index];
            if (e != null && e.currentHp > 0)
                return e;
        }
        return null;
    }

    private class EnemyAttack
    {
        public Enemy enemy;
        public int damage;
    }
}