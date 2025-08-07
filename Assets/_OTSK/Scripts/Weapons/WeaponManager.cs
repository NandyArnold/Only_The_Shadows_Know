// WeaponManager.cs
using UnityEngine;

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

    private GameObject _mainHandInstance;
    private GameObject _offHandInstance;

    private GameObject _currentWeaponObject;
    private PlayerAnimationController _animController;
    private Coroutine _switchWeaponCoroutine;


    private void Awake()
    {
        _animController = GetComponentInParent<PlayerAnimationController>();
    }





    // This is the public method that PlayerCombat will call.
    public void EquipNewWeapon(WeaponSO weapon)
    {
        if (_mainHandInstance != null) Destroy(_mainHandInstance);
        if (_offHandInstance != null) Destroy(_offHandInstance);

        // Spawn and attach the main-hand weapon using its specific socket.
        if (weapon.mainHandPrefab != null)
        {
            Transform socket = GetSocket(weapon.mainHandEquipSocket);
            _mainHandInstance = Instantiate(weapon.mainHandPrefab, socket);
        }

        // Spawn and attach the off-hand weapon using its specific socket.
        if (weapon.offHandWeaponPrefab != null)
        {
            Transform offHandSocket = GetSocket(weapon.offHandEquipSocket);
            _offHandInstance = Instantiate(weapon.offHandWeaponPrefab, offHandSocket);
        }
    }

    private Transform GetSocket(SocketType socketType)
    {
        switch (socketType)
        {
            case SocketType.Hand_R: return handSocketR;
            case SocketType.Hand_L: return handSocketL;
            case SocketType.Back: return backSocket;
            case SocketType.Hip_L: return hipSocketL;
            case SocketType.Hip_R: return hipSocketR;
            default: return null;
        }
    }
}
