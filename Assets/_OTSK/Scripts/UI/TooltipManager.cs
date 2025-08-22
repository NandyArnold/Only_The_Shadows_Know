using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;


[System.Serializable]
public class MovementAbilityInfo
{
    public string abilityName;
    [TextArea] public string abilityDescription;
}


public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [SerializeField] private GameObject tooltipPanel;

    [Header("Weapon UI References")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI weaponDescriptionText;
    [SerializeField] private Button lmbButton;
    [SerializeField] private Button rmbButton;
    [SerializeField] private Button mmbButton;
    [Header("Weapon Tooltip References")]
    [SerializeField] private GameObject weaponAttackTooltipPanel;
    [SerializeField] private TextMeshProUGUI attackNameText;
    [SerializeField] private TextMeshProUGUI attackDescriptionText;

    [Header("Skill UI References")]
    [SerializeField] private GameObject skillSlotPrefab;
    [SerializeField] private Transform skillListParent;
    [SerializeField] private GameObject skillDescriptionPanel;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillTypeText;
    [SerializeField] private TextMeshProUGUI skillUsageCondotionsText; // This is the description text in the skill panel
    [SerializeField] private TextMeshProUGUI skillManaCostText;
    [SerializeField] private TextMeshProUGUI skillManaCostOverTimeText;
    [SerializeField] private TextMeshProUGUI skillCastModeText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;


    [Header("Movement UI")]
    [SerializeField] private GameObject movementSlotPrefab;
    [SerializeField] private Transform movementListParent;
    [SerializeField] private List<MovementAbilityInfo> movementAbilities;
    [SerializeField] private GameObject descriptionTooltipPanel;
    [SerializeField] private TextMeshProUGUI descriptionNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private WeaponSO _currentWeaponForTooltip;

    private PlayerInputHandler _playerInputHandler;
    private PlayerCombat _playerCombat;
    private PlayerSkillController _playerSkillController;

    private bool _isPanelOpen = false;

    private bool _isToggling = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnPlayerReady += HandlePlayerReady;
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerReady -= HandlePlayerReady;

        if (_playerInputHandler != null)
            _playerInputHandler.OnToggleDetailsInput -= TogglePanel;
    }

    private void HandlePlayerReady()
    {
        var player = GameManager.Instance.Player;
        if (player == null) return;

        // Now it is safe to get all components and populate the UI
        _playerInputHandler = player.GetComponent<PlayerInputHandler>();
        if (_playerInputHandler != null)
        {
            _playerInputHandler.OnToggleDetailsInput += TogglePanel;
        }

        _playerCombat = player.GetComponent<PlayerCombat>();
        if (_playerCombat != null)
        {
            _playerCombat.OnWeaponSwitched += HandleWeaponSwitched;
            HandleWeaponSwitched(_playerCombat.CurrentWeapon);
        }

        _playerSkillController = player.GetComponent<PlayerSkillController>();

        PopulateSkillPanel();
        PopulateMovementPanel();
    }

    private void TogglePanel()
    {
        // If we are already in the middle of a toggle from this frame, ignore this input.
        if (_isToggling) return;

        _isPanelOpen = !_isPanelOpen;
        tooltipPanel.SetActive(_isPanelOpen);

        if (_isPanelOpen)
        {
            UISoundPlayer.Instance.PlayMenuOpenSound();
            GameManager.Instance.UpdateGameState(GameState.Details);
        }
        else
        {
            UISoundPlayer.Instance.PlayMenuCloseSound();
            GameManager.Instance.UpdateGameState(GameState.Gameplay);
        }

        // Start the cooldown coroutine.
        StartCoroutine(ToggleCooldownRoutine());
    }

    // This coroutine creates a very short "debounce" period.
    private IEnumerator ToggleCooldownRoutine()
    {
        _isToggling = true;
        // Wait for the end of the current frame.
        yield return new WaitForEndOfFrame();
        _isToggling = false;
    }

    private void HandleWeaponSwitched(WeaponSO newWeapon)
    {
        _currentWeaponForTooltip = newWeapon;
        if (newWeapon == null) return;

        // Update the main text fields
        weaponNameText.text = newWeapon.weaponName;
        weaponDescriptionText.text = newWeapon.weaponDescription;

        // Update the button labels with the attack names
        lmbButton.GetComponentInChildren<TextMeshProUGUI>().text = newWeapon.primaryAttackName;
        rmbButton.GetComponentInChildren<TextMeshProUGUI>().text = newWeapon.secondaryAttackName;

        // Show or hide the MMB button based on whether the weapon is Animancy
        bool hasTertiary = (newWeapon is AnimancySO);
        mmbButton.gameObject.SetActive(hasTertiary);
        if (hasTertiary)
        {
            mmbButton.GetComponentInChildren<TextMeshProUGUI>().text = newWeapon.tertiaryAttackName;
        }
    }
    public void ShowWeaponAttackTooltip(int attackIndex)
    {
        if (_currentWeaponForTooltip == null) return;

        string nameToShow = "";
        string descriptionToShow = "";

        // Get the correct name and description based on the button index
        if (attackIndex == 0) // LMB
        {
            nameToShow = _currentWeaponForTooltip.primaryAttackName;
            descriptionToShow = _currentWeaponForTooltip.primaryAttackDescription;
        }
        else if (attackIndex == 1) // RMB
        {
            nameToShow = _currentWeaponForTooltip.secondaryAttackName;
            descriptionToShow = _currentWeaponForTooltip.secondaryAttackDescription;
        }
        else if (attackIndex == 2) // MMB
        {
            nameToShow = _currentWeaponForTooltip.tertiaryAttackName;
            descriptionToShow = _currentWeaponForTooltip.tertiaryAttackDescription;
        }

        attackNameText.text = nameToShow;
        attackDescriptionText.text = descriptionToShow;
        weaponAttackTooltipPanel.SetActive(true);
    }

    // NEW: This method is called when the mouse leaves a button.
    public void HideWeaponAttackTooltip()
    {
        weaponAttackTooltipPanel.SetActive(false);
    }

    private void PopulateSkillPanel()
    {
        if (_playerSkillController == null || skillSlotPrefab == null) return;

        foreach (Transform child in skillListParent) Destroy(child.gameObject);

        foreach (var skill in _playerSkillController.GetEquippedSkills())
        {
            if (skill == null) continue;

            // As you requested, we will not show DeathZone in the skills list.
            if (skill.skillID == SkillIdentifier.DeathZone) continue;

            GameObject slotGO = Instantiate(skillSlotPrefab, skillListParent);

            // Get the helper script from the new instance
            SkillSlotUI slotUI = slotGO.GetComponent<SkillSlotUI>();

            // Use the references from the helper script to populate the UI
            slotUI.skillIconImage.sprite = skill.icon;
            slotUI.keybindText.text = skill.keybindText;

            // Add event triggers from code to handle hover
            EventTrigger trigger = slotGO.GetComponent<EventTrigger>() ?? slotGO.AddComponent<EventTrigger>();

            //AddEventTrigger(trigger, EventTriggerType.PointerEnter, () => UISoundPlayer.Instance.PlayHoverSound());

            AddEventTrigger(trigger, EventTriggerType.PointerEnter, () => ShowSkillTooltip(skill));
            AddEventTrigger(trigger, EventTriggerType.PointerExit, HideSkillTooltip);
        }
    }

    
    private void ShowSkillTooltip(SkillSO skill)
    {
        skillNameText.text =$"Name: { skill.skillName}";
        skillTypeText.text = $"Type: {skill.skillType}";
        skillUsageCondotionsText.text = $"Usage Condition: {skill.usageCondition}";
        skillManaCostText.text = $"Mana Cost: {skill.manaCost}";
        skillManaCostOverTimeText.text = $"Mana Cost Over Time: {skill.manaCostOverTime}";
        skillCastModeText.text = $"Cast Mode: {skill.castMode}";
        skillDescriptionText.text =$"Description: {skill.description}";

        skillDescriptionPanel.SetActive(true);
    }

    // This method is called when you stop hovering
    private void HideSkillTooltip()
    {
        skillDescriptionPanel.SetActive(false);
    }

    private void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction action)
    {
        // Create a new event trigger entry
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        // Add a listener to the trigger's callback
        entry.callback.AddListener((data) => action());
        // Add the new trigger entry to the list of triggers
        trigger.triggers.Add(entry);
    }

    private void PopulateMovementPanel()
    {
        if (movementSlotPrefab == null || movementListParent == null) return;

        // Clear any old slots first
        foreach (Transform child in movementListParent)
        {
            Destroy(child.gameObject);
        }

        // Create a new button for each ability in the list
        foreach (var move in movementAbilities)
        {
            GameObject slotGO = Instantiate(movementSlotPrefab, movementListParent);
            slotGO.GetComponentInChildren<TextMeshProUGUI>().text = move.abilityName;

            // Add event triggers from code to handle hovering
            EventTrigger trigger = slotGO.GetComponent<EventTrigger>();

            //AddEventTrigger(trigger, EventTriggerType.PointerEnter, () => UISoundPlayer.Instance.PlayHoverSound());

            AddEventTrigger(trigger, EventTriggerType.PointerEnter, () => ShowDescriptionTooltip(move.abilityName, move.abilityDescription));
            AddEventTrigger(trigger, EventTriggerType.PointerExit, HideDescriptionTooltip);
        }
    }

    public void ShowDescriptionTooltip(string title, string description)
    {
        if (descriptionTooltipPanel == null) return;

        descriptionNameText.text = title;
        descriptionText.text = description;
        descriptionTooltipPanel.SetActive(true);
    }

    // NEW: This method is called when you stop hovering.
    public void HideDescriptionTooltip()
    {
        if (descriptionTooltipPanel == null) return;

        descriptionTooltipPanel.SetActive(false);
    }

}