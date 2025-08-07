using System.Collections;
using UnityEngine;

public class CoroutineHost : MonoBehaviour
{
    public void Run(IEnumerator routine)
    {
        StartCoroutine(ExecuteAndDestroy(routine));
    }

    public IEnumerator ExecuteAndDestroy(IEnumerator routine)
    {
        // Wait for the skill's own coroutine to finish
        yield return StartCoroutine(routine);

        // Once it's done, destroy the temporary GameObject this script is on
        Destroy(gameObject);
    }
}