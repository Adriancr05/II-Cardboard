using UnityEngine;
using UnityEngine.UI;

public class GazeInteractor : MonoBehaviour
{
    public float maxDistance = 30f;
    public float dwellTime = 1.5f;
    public Image progressImage;     // opcional: Image con Fill Method Radial 360
    public Transform player;        // arrastra el objeto Player

    GazeTarget current;
    float timer;

    void Update()
    {
        GazeTarget hitTarget = null;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxDistance))
            hitTarget = hit.collider.GetComponentInParent<GazeTarget>();

        if (hitTarget != current)
        {
            if (current != null) current.OnGazeExit();
            current = hitTarget;
            timer = 0f;
            if (current != null) current.OnGazeEnter();
        }

        // Cada objetivo puede pedir su propio tiempo de mirada
        float required = current != null ? current.GetDwellTime(dwellTime) : dwellTime;
        required = Mathf.Max(required, 0.05f);

        if (current != null && current.CanBeSelected(player))
        {
            timer += Time.deltaTime;
            if (timer >= required)
            {
                current.OnGazeSelect(player);
                timer = 0f;
            }
        }
        else timer = 0f;

        if (progressImage != null) progressImage.fillAmount = timer / required;
    }
}
