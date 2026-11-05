using UnityEngine;

// Objeto que, al ser mirado, hace que todos los recolectables vuelvan a su sitio
// y se dirijan hacia el jugador. Debe llevar un Collider.
public class CollectableRetriever : GazeTarget
{
    [Tooltip("Segundos de mirada necesarios (menor = más fácil)")]
    public float retrieveDwellTime = 0.4f;

    [Tooltip("Radio (m) de una zona invisible alrededor del objeto que también cuenta como mirarlo. 0 = desactivada")]
    public float extraHitRadius = 2f;

    [Tooltip("Segundos de espera entre activaciones, para no reiniciar los objetos sin parar")]
    public float cooldown = 3f;

    float nextAllowedTime;

    void Awake()
    {
        if (extraHitRadius <= 0f) return;

        // Zona invisible más grande que el objeto, centrada en su parte visible
        GameObject zone = new GameObject("RetrieverHitZone");
        zone.transform.SetParent(transform, false);

        Renderer r = GetComponentInChildren<Renderer>();
        zone.transform.position = r != null ? r.bounds.center : transform.position;

        SphereCollider sc = zone.AddComponent<SphereCollider>();
        sc.isTrigger = true;

        Vector3 s = transform.lossyScale;
        float maxScale = Mathf.Max(s.x, Mathf.Max(s.y, s.z));
        sc.radius = extraHitRadius / Mathf.Max(maxScale, 0.0001f);
    }

    public override float GetDwellTime(float defaultTime)
    {
        return retrieveDwellTime;
    }

    public override bool CanBeSelected(Transform player)
    {
        return Time.time >= nextAllowedTime;
    }

    public override void OnGazeSelect(Transform player)
    {
        nextAllowedTime = Time.time + cooldown;

        foreach (CollectableItem item in CollectableItem.AllItems)
        {
            item.FlyToPlayer(true);
        }
    }
}
