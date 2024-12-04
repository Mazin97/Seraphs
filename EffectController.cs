using UnityEngine;

public class sc : MonoBehaviour
{
    public float EffectDuration;

    void Start()
    {
        Destroy(gameObject, EffectDuration);
    }
}
