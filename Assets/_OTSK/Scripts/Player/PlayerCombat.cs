using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Unity.Cinemachine;

public class PlayerCombat : MonoBehaviour
{
    // Public properties for other scripts to access
    public CinemachineBrain Brain { get; private set; }
    public PlayerAnimationController PlayerAnimationController => playerAnimationController;
    public Transform FirePoint => firePoint;
    public bool IsFocused => _isFocused;
    public bool IsAiming => _isAiming;
    public WeaponSO CurrentWeapon => _currentWeapon;
    public NoiseSettingsSO NoiseSettings => noiseSettings;
    public PlayerHealthManaNoise HealthManaNoise => playerHealthManaNoise;
    public Transform HandSocketR => handSocketR;
    public Transform HandSocketL => handSocketL;

    [Header("Component References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerAnimationController playerAnimationController;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private Rig ikRig;
    [SerializeField] private MultiAimConstraint aimConstraint;
    [SerializeField] private PlayerSkillController playerSkillController;

    [Header("Data References")]
    [SerializeField] private NoiseSettingsSO noiseSettings;
    [SerializeField] private PlayerHealthManaNoise playerHealthManaNoise;

    [Header("Combat Points")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform handSocketR;
    [SerializeField] private Transform handSocketL;

    [Header("Weapon Settings")]
    [SerializeField] private List<WeaponSO> availableWeapons;

    public event Action<bool> OnAimStateChanged;
    public event Action<bool> OnFocusStateChanged;
    public event Action<WeaponSO> OnWeaponSwitched;
    public event Action OnBowFire;
    public event Action OnFocusedShotFired;

    private WeaponSO _currentWeapon;
    private DaggerAnimation _daggerAnimation;
    private BowAnimation _bowAnimation;
    private AnimancyAnimation _animancyAnimation;
    private bool _isAiming = false;
    private bool _isFocused = false;
    private Coroutine _rigWeightCoroutine;
    private float _lastAttackTime;

    private void Awake()
    {
        // Get component references
        playerInputHandler = GetComponent<PlayerInputHandler>();
        playerAnimationController = GetComponent<PlayerAnimationController>();
        weaponManager = GetComponent<WeaponManager>();
        playerSkillController = GetComponent<PlayerSkillController>();
        _daggerAnimation = GetComponent<DaggerAnimation>();
        _bowAnimation = GetComponent<BowAnimation>();
        _animancyAnimation = GetComponent<AnimancyAnimation>();
        ikRig = GetComponentInChildren<Rig>();
        Brain = Camera.main.GetComponent<CinemachineBrain>();

        playerHealthManaNoise = GetComponent<PlayerHealthManaNoise>();

        if (GameManager.Instance != null && GameManager.Instance.AimTarget != null)
        {
            var sourceList = new WeightedTransformArray();
            sourceList.Add(new WeightedTransform(GameManager.Instance.AimTarget, 1f));
            if (aimConstraint != null) { aimConstraint.data.sourceObjects = sourceList; }
            var rigBuilder = GetComponent<RigBuilder>();
            if (rigBuilder != null) rigBuilder.Build();
        }

        if (availableWeapons != null && availableWeapons.Count > 0)
        {
            SwitchWeapon(availableWeapons[0]);
        }
    }

    private void OnEnable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnPrimaryAttackInput += HandlePrimaryAttack;
            playerInputHandler.OnSecondaryAttackInput += HandleSecondaryAttack;
            playerInputHandler.OnSecondaryAttackPressed += HandleSecondaryAttackPress;
            playerInputHandler.OnSecondaryAttackReleased += HandleSecondaryAttackRelease;
            playerInputHandler.OnTertiaryAttackInput += HandleTertiaryAttack;
            playerInputHandler.OnWeapon1Input += HandleWeapon1Switch;
            playerInputHandler.OnWeapon2Input += HandleWeapon2Switch;
            playerInputHandler.OnWeapon3Input += HandleWeapon3Switch;
        }
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnPrimaryAttackInput -= HandlePrimaryAttack;
            playerInputHandler.OnSecondaryAttackInput -= HandleSecondaryAttack;
            playerInputHandler.OnSecondaryAttackPressed -= HandleSecondaryAttackPress;
            playerInputHandler.OnSecondaryAttackReleased -= HandleSecondaryAttackRelease;
            playerInputHandler.OnTertiaryAttackInput -= HandleTertiaryAttack;
            playerInputHandler.OnWeapon1Input -= HandleWeapon1Switch;
            playerInputHandler.OnWeapon2Input -= HandleWeapon2Switch;
            playerInputHandler.OnWeapon3Input -= HandleWeapon3Switch;
        }
    }


    public void SwitchWeaponByIndex(int index)
    {
        if (index < 0 || index >= availableWeapons.Count) return;
        SwitchWeapon(availableWeapons[index]);
    }

    public void SwitchWeapon(WeaponSO newWeapon)
    {
        if (newWeapon == null || newWeapon == _currentWeapon) return;

        _currentWeapon = newWeapon;

        OnWeaponSwitched?.Invoke(_currentWeapon);
        weaponManager.EquipNewWeapon(newWeapon);

        // Determine if the weapon uses the IK aiming rig.
        bool usesAimIK = (newWeapon is BowSO);
        UpdateRigWeight(usesAimIK);

        // --- SINGLE, CLEAN LOGIC BLOCK ---
        if (newWeapon is BowSO)
        {
            playerAnimationController.SetWeaponAnimation(_bowAnimation);
            playerAnimationController.SetWeaponType(2);
            SetAimState(true);
        }
        else if (newWeapon is DaggerSO)
        {
            playerAnimationController.SetWeaponAnimation(_daggerAnimation);
            playerAnimationController.SetWeaponType(1);
            SetAimState(false);
        }
        else if (newWeapon is AnimancySO)
        {
            
            playerAnimationController.SetWeaponAnimation(_animancyAnimation);
            playerAnimationController.SetWeaponType(3);
            SetAimState(false);
        }

    }

    private void SetAimState(bool isAiming)
    {
        
        _isAiming = isAiming;
        OnAimStateChanged?.Invoke(_isAiming);
        playerAnimationController.SetAimingState(_isAiming);
        if (!_isAiming && _isFocused)
        {
            SetFocusState(false);
        }
    }

    private void HandlePrimaryAttack()
    {
        if (_currentWeapon == null) return;
        if (Time.time < _lastAttackTime + _currentWeapon.timeBetweenAttacks) return;

        // This is now simple. It just tells the weapon to perform its primary attack.
        // The weapon itself will figure out if it's a focused shot or not.
        _currentWeapon.PrimaryAttack(this);

        if (_currentWeapon is BowSO) OnBowFire?.Invoke();
        _lastAttackTime = Time.time;
    }

    private void HandleSecondaryAttack()
    {
        if (_currentWeapon == null || _currentWeapon is BowSO) return;
        if (Time.time < _lastAttackTime + _currentWeapon.timeBetweenAttacks) return;

        _currentWeapon.SecondaryAttack(this);
        _lastAttackTime = Time.time;
    }

    private void HandleSecondaryAttackPress()
    {
        if (_currentWeapon is BowSO && _isAiming) SetFocusState(true);
    }

    // This handler is for RELEASING A HOLD (for the Bow)
    private void HandleSecondaryAttackRelease()
    {
        if (_currentWeapon is BowSO && _isAiming && _isFocused)
        {
            SetFocusState(false);
        }
    }

    private void HandleTertiaryAttack()
    {
        if (_currentWeapon is AnimancySO)
        {
            // First, trigger the animation
            playerAnimationController.TriggerTertiaryAttack();
            // Then, activate the skill
            playerSkillController.TryActivateSkill(SkillIdentifier.DeathZone);
        }
    }

    // Using named methods for weapon switching to ensure stable unsubscription
    private void HandleWeapon1Switch() => SwitchWeaponByIndex(0);
    private void HandleWeapon2Switch() => SwitchWeaponByIndex(1);
    private void HandleWeapon3Switch() => SwitchWeaponByIndex(2);
    //private void HandleWeapon1Switch() => SwitchWeapon(availableWeapons[0]);
    //private void HandleWeapon2Switch() => SwitchWeapon(availableWeapons[1]);
    //private void HandleWeapon3Switch() => SwitchWeapon(availableWeapons[2]);
    private void UpdateRigWeight(bool enable)
    {
        if (_rigWeightCoroutine != null)
        {
            StopCoroutine(_rigWeightCoroutine);
        }
        float targetWeight = enable ? 1f : 0f;
        _rigWeightCoroutine = StartCoroutine(FadeRigWeight(targetWeight, 0.25f));
    }

    // NEW: The coroutine that smoothly changes the weight over time.
    private IEnumerator FadeRigWeight(float targetWeight, float duration)
    {
        float startTime = Time.time;
        float startWeight = ikRig.weight;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            ikRig.weight = Mathf.Lerp(startWeight, targetWeight, t);
            yield return null;
        }

        ikRig.weight = targetWeight;
    }
    private void SetFocusState(bool isFocused)
    {
        if (_isFocused == isFocused) return;
        _isFocused = isFocused;
        OnFocusStateChanged?.Invoke(_isFocused);
    }
}