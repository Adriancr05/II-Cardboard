using UnityEngine;

public abstract class GazeTarget : MonoBehaviour
{
    public virtual void OnGazeEnter() { }
    public virtual void OnGazeExit() { }
    public virtual bool CanBeSelected(Transform player) { return true; }

    // Tiempo de mirada necesario para seleccionar. Por defecto el del GazeInteractor;
    // un objeto puede devolver otro valor para ser más fácil o más difícil de activar.
    public virtual float GetDwellTime(float defaultTime) { return defaultTime; }

    public abstract void OnGazeSelect(Transform player);
}
