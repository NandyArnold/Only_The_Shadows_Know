// Create this new script, ChargeableItemSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Item_NewChargeable", menuName = "Only The Shadows Know/Items/Chargeable Item")]
public class ChargeableItemSO : ScriptableObject
{
    [Tooltip("The maximum number of charges the player can hold for this item.")]
    public int maxCharges = 20;
    [Tooltip("The number of charges the player starts with.")]
    public int startingCharges = 10;
}