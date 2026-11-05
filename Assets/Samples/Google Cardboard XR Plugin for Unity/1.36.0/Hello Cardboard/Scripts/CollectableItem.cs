using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Objeto recolectable con la vista. Debe llevar un Collider (mejor Is Trigger).
public class CollectableItem : GazeTarget
{
    // Lista de TODOS los items (activos o no) que existían activos al empezar la escena
    public static readonly List<CollectableItem> AllItems = new List<CollectableItem>();

    [Tooltip("Distancia máxima jugador-objeto para poder recolectarlo con la vista")]
    public float collectDistance = 40f;
    public float speed = 3f;
    [Tooltip("Distancia a la cámara a la que se considera recolectado")]
    public float arriveDistance = 1f;

    Vector3 initialPosition;
    Coroutine routine;

    // Evita listas con restos si desactivas "Domain Reload" en el editor
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { AllItems.Clear(); }

    void Awake()
    {
        AllItems.Add(this);
        initialPosition = transform.position;
    }

    void OnDestroy() { AllItems.Remove(this); }

    void OnDisable() { routine = null; }   // al desactivarse se paran las corrutinas

    // Solo se puede recolectar si el jugador está cerca y el objeto no está ya volando
    public override bool CanBeSelected(Transform player)
    {
        return routine == null &&
               Vector3.Distance(transform.position, player.position) <= collectDistance;
    }

    public override void OnGazeSelect(Transform player)
    {
        FlyToPlayer(false);
    }

    // resetPosition = true: vuelve a su sitio original y reaparece (lo usa el Retriever)
    public void FlyToPlayer(bool resetPosition)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        if (resetPosition)
        {
            transform.position = initialPosition;
            gameObject.SetActive(true);
        }

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Fly(cam.transform));
    }

    IEnumerator Fly(Transform target)
    {
        // Se sigue a la cámara en cada frame, así funciona aunque el jugador se mueva
        while (Vector3.Distance(transform.position, target.position) > arriveDistance)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, target.position, speed * Time.deltaTime);
            yield return null;
        }

        routine = null;
        gameObject.SetActive(false);   // recolectado
    }
}
