
using UnityEngine;

public static class ParticleEffects
{
    public static GameObject PlayOnce(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation = default)
    {
        if (prefab == null)
            return null;

        GameObject effect = Object.Instantiate(prefab, position, rotation);

        ParticleSystem[] systems =
            effect.GetComponentsInChildren<ParticleSystem>(true);

        float lifetime = 0f;

        foreach (ParticleSystem system in systems)
        {
            system.Play(true);

            var main = system.main;

            lifetime = Mathf.Max(
                lifetime,
                main.duration + main.startLifetime.constantMax
            );
        }

        Object.Destroy(effect, lifetime + 0.25f);

        return effect;
    }

    public static GameObject Attach(
        GameObject prefab,
        Transform target,
        Vector3 localPosition = default)
    {
        if (prefab == null || target == null)
            return null;

        GameObject effect = Object.Instantiate(prefab, target);

        effect.transform.localPosition = localPosition;
        effect.transform.localRotation = Quaternion.identity;

        foreach (ParticleSystem system in
                 effect.GetComponentsInChildren<ParticleSystem>(true))
        {
            system.Play(true);
        }

        return effect;
    }

    public static void StopAndDestroy(GameObject effect)
    {
        if (effect == null)
            return;

        effect.transform.SetParent(null, true);

        ParticleSystem[] systems =
            effect.GetComponentsInChildren<ParticleSystem>(true);

        float lifetime = 0f;

        foreach (ParticleSystem system in systems)
        {
            var main = system.main;

            lifetime = Mathf.Max(
                lifetime,
                main.startLifetime.constantMax
            );

            system.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        Object.Destroy(effect, lifetime + 0.25f);
    }
}
