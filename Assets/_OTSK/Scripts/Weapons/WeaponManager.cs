// WeaponManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Manages the visual representation of weapons: spawning prefabs and attaching them to sockets.
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("Socket Transforms")]
    [SerializeField] private Transform handSocketR;
    [SerializeField] private Transform handSocketL;
    [SerializeField] private Transform backSocket;
    [SerializeField] private Transform hipSocketL;
    [SerializeField] private Transform hipSocketR;

    //private GameObject _mainHandInstance;
    //private GameObject _offHandInstance;
    // A dictionary to track all our weapon instances (both sheathed and equipped)
    private readonly Dictionary<WeaponSO, (GameObject main, GameObject off)> _weaponInstances = new Dictionary<WeaponSO, (GameObject, GameObject)>();


    private GameObject _currentWeaponObject;
    private PlayerAnimationController _animController;
    private Coroutine _switchWeaponCoroutine;
    private WeaponSO _currentlyEquippedWeapon;

    private void Awake()
    {
        _animController = GetComponentInParent<PlayerAnimationController>();
    }

    // Called by PlayerCombat to give this manager the necessary data
    public void Initialize(List<WeaponSO> availableWeapons, WeaponSO defaultWeapon)
    {
        // Pre-spawn all weapons and place them in their sheath sockets
        foreach (var weapon in availableWeapons)
        {
            InstantiateAndSheatheWeapon(weapon);
        }

        // Equip the default weapon
        EquipNewWeapon(defaultWeapon);
    }

 
  

    public void EquipNewWeapon(WeaponSO newWeapon)
    {
        if (_switchWeaponCoroutine != null) StopCoroutine(_switchWeaponCoroutine);
        _switchWeaponCoroutine = StartCoroutine(SwitchWeaponRoutine(_currentlyEquippedWeapon, newWeapon));
        _currentlyEquippedWeapon = newWeapon;
    }

    private IEnumerator SwitchWeaponRoutine(WeaponSO oldWeapon, WeaponSO newWeapon)
    {
        // 1. UNEQUIP the old weapon
        if (oldWeapon != null)
        {
            if (!string.IsNullOrEmpty(oldWeapon.unequipTriggerName))
            {
                _animController.PlayAnimationTrigger(oldWeapon.unequipTriggerName);
                yield return new WaitForSeconds(oldWeapon.unequipDuration);
            }


            // Move the weapon(s) from hand to sheath
            if (_weaponInstances.TryGetValue(oldWeapon, out var oldInstances))
            {
                ParentToSocket(oldInstances.main, GetSocketTransform(oldWeapon.mainHandSheathSocket));
                ParentToSocket(oldInstances.off, GetSocketTransform(oldWeapon.offHandSheathSocket));
            }
        }

        // 2. EQUIP the new weapon
        if (newWeapon != null)
        {
           

            // Play the equip animation and wait for it to finish
            if (!string.IsNullOrEmpty(newWeapon.equipTriggerName))
            {
                _animController.PlayAnimationTrigger(newWeapon.equipTriggerName);
                yield return new WaitForSeconds(newWeapon.equipDuration);
            }

            // Move the weapon(s) from sheath to hand
            if (_weaponInstances.TryGetValue(newWeapon, out var newInstances))
            {
                ParentToSocket(newInstances.main, GetSocketTransform(newWeapon.mainHandEquipSocket));
                ParentToSocket(newInstances.off, GetSocketTransform(newWeapon.offHandEquipSocket));
            }
        }
        yield return null;
    }

    private void InstantiateAndSheatheWeapon(WeaponSO weapon)
    {
        if (_weaponInstances.ContainsKey(weapon)) return;

        GameObject mainHandInstance = null;
        if (weapon.mainHandPrefab != null)
        {
            mainHandInstance = Instantiate(weapon.mainHandPrefab, GetSocketTransform(weapon.mainHandSheathSocket));
        }

        GameObject offHandInstance = null;
        if (weapon.offHandPrefab != null)
        {
            offHandInstance = Instantiate(weapon.offHandPrefab, GetSocketTransform(weapon.offHandSheathSocket));
        }

        _weaponInstances.Add(weapon, (mainHandInstance, offHandInstance));
    }

    private void ParentToSocket(GameObject weaponObject, Transform socket)
    {
        if (weaponObject == null) return;

        if (socket != null)
        {
            // Set the parent. The 'false' parameter ensures its position is adjusted correctly.
            weaponObject.transform.SetParent(socket, false);

            // REMOVE these two lines, as they were overriding your prefab's transform.
            // weaponObject.transform.localPosition = Vector3.zero;
            // weaponObject.transform.localRotation = Quaternion.identity;

            weaponObject.SetActive(true);
        }
        else
        {
            // If the socket is "None", we just deactivate the object.
            weaponObject.SetActive(false);
        }
    }

    private Transform GetSocketTransform(SheathSocket socket)
    {
        switch (socket)
        {
            case SheathSocket.Hip_L: return hipSocketL;
            case SheathSocket.Hip_R: return hipSocketR;
            case SheathSocket.Back: return backSocket;
            default: return null;
        }
    }

    private Transform GetSocketTransform(EquipSocket socket)
    {
        return (socket == EquipSocket.Right_Hand) ? handSocketR : handSocketL;
    }
}



    // This is the public method that PlayerCombat will call.
    //public void EquipNewWeapon(WeaponSO weapon)
    //{
    //    if (_mainHandInstance != null) Destroy(_mainHandInstance);
    //    if (_offHandInstance != null) Destroy(_offHandInstance);

    //    // Spawn and attach the main-hand weapon using its specific socket.
    //    if (weapon.mainHandPrefab != null)
    //    {
    //        Transform socket = GetSocket(weapon.mainHandEquipSocket);
    //        _mainHandInstance = Instantiate(weapon.mainHandPrefab, socket);
    //    }

    //    // Spawn and attach the off-hand weapon using its specific socket.
    //    if (weapon.offHandWeaponPrefab != null)
    //    {
    //        Transform offHandSocket = GetSocket(weapon.offHandEquipSocket);
    //        _offHandInstance = Instantiate(weapon.offHandWeaponPrefab, offHandSocket);
    //    }
    //}

    //private Transform GetSocket(SocketType socketType)
    //{
    //    switch (socketType)
    //    {
    //        case SocketType.Hand_R: return handSocketR;
    //        case SocketType.Hand_L: return handSocketL;
    //        case SocketType.Back: return backSocket;
    //        case SocketType.Hip_L: return hipSocketL;
    //        case SocketType.Hip_R: return hipSocketR;
    //        default: return null;
    //    }
    //}

