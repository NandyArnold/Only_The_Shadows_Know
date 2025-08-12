// Create this new script, IKTargetFinder.cs
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IKTargetFinder : MonoBehaviour
{
    [Tooltip("The Multi-Aim Constraint that needs its target set.")]
    [SerializeField] private MultiAimConstraint aimConstraint;

    private void Start()
    {
        if (aimConstraint == null)
        {
            aimConstraint = GetComponent<MultiAimConstraint>();
        }

        // Find the persistent aim target in the scene by its tag.
        GameObject targetObject = GameObject.FindGameObjectWithTag("AimTarget");

        if (targetObject != null)
        {
            // Create a new source list for the constraint.
            var sourceList = new WeightedTransformArray();
            sourceList.Add(new WeightedTransform(targetObject.transform, 1f));

            // Assign the found target to the constraint.
            aimConstraint.data.sourceObjects = sourceList;

            // Rebuild the rig with the new data.
            var rigBuilder = GetComponentInParent<RigBuilder>();
            if (rigBuilder != null)
            {
                rigBuilder.Build();
            }
        }
        else
        {
            Debug.LogError("IKTargetFinder: Could not find GameObject with 'AimTarget' tag!", this);
        }
    }
}