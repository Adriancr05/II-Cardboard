# Práctica 5 – Escenas Cardboard

Asignatura: Interfaces Inteligentes  
Universidad: Universidad de La Laguna  
Titulación: Grado en Ingeniería Informática  
Curso: 2026/2027  
Fecha: 23 de octubre de 2026

Escena de realidad virtual para **Google Cardboard** en Unity. El jugador recorre un terreno, **recolecta monedas con la mirada** y puede **recuperarlas todas** mirando un cofre.

![](Demo/Demo.gif)

## Contenido

1. [Qué hace la aplicación](#qué-hace-la-aplicación)
2. [Escena y assets](#escena-y-assets)
3. [Scripts](#scripts)
4. [Jerarquía de la escena](#jerarquía-de-la-escena)
5. [Configuración del proyecto](#configuración-del-proyecto)
6. [Generar el APK](#generar-el-apk)
7. [Probar en el editor](#probar-en-el-editor)
8. [Problemas encontrados](#problemas-encontrados)
9. [Entrega](#entrega)

## Qué hace la aplicación

1. **Movimiento.** El jugador se desplaza hacia donde mira. Un toque en la pantalla (el botón del Cardboard) activa o desactiva el avance. También admite un mando Bluetooth con el stick izquierdo.
2. **Recolección con la vista.** Un rayo sale del centro de la cámara. Si el jugador está a menos de `collectDistance` de una moneda y la mantiene en la mirada durante `dwellTime` segundos, la moneda vuela hacia él y desaparece al llegar.
3. **Recuperación.** El **cofre** es el objeto recuperador. Al mirarlo durante un tiempo corto, todas las monedas vuelven a su posición original y se dirigen hacia el jugador.

Un punto blanco en el centro de la vista (en el editor) marca hacia dónde apunta el rayo.

## Escena y assets

La escena usa un **terreno** (`New Terrain`) y paquetes de la Asset Store:

| Paquete | Uso |
|---|---|
| **Simple Nature Pack** | Árboles, rocas, arbustos y vegetación que decoran el terreno |
| **Stylized Coin Pack** (PolyKebap) | Monedas doradas que hay que recolectar |
| **Animated PBR Chest Demo** | Cofre animado con efectos, que hace de objeto recuperador |

Se usa el render pipeline **URP**.

## Scripts

Están en `Assets/Samples/Google Cardboard XR Plugin for Unity/1.36.0/Hello Cardboard/Scripts/`.

| Script | Función |
|---|---|
| `CardboardStartup.cs` | Inicializa el plugin Cardboard (parámetros del visor, botón de cierre, recentrado con pulsación larga). |
| `PlayerMovement.cs` | Movimiento con `CharacterController`: toque en pantalla (alterna avanzar/parar) y mando Bluetooth. |
| `GazeTarget.cs` | Clase base abstracta de todo objeto que se puede mirar: `OnGazeEnter`, `OnGazeExit`, `CanBeSelected`, `GetDwellTime` y `OnGazeSelect`. |
| `GazeInteractor.cs` | Va en la cámara. Lanza un `Physics.Raycast` hacia delante y cuenta el tiempo de mirada (dwell). Al completarlo llama a `OnGazeSelect` del objeto. Admite una imagen radial de progreso opcional. |
| `CollectableItem.cs` | Moneda recolectable. Solo se puede seleccionar si el jugador está cerca. Al seleccionarla vuela hacia la cámara y se desactiva. Mantiene una lista estática con todas las monedas. |
| `CollectableRetriever.cs` | Cofre recuperador. Tiene un tiempo de mirada propio y más corto, una zona de detección ampliada y un enfriamiento para no dispararse sin parar. Al activarse hace volar todas las monedas hacia el jugador. |
| `EditorLook.cs` | Solo en el editor: gira la cámara con el botón derecho del ratón y dibuja un punto en el centro de la pantalla. |

### Parámetros principales (Inspector)

| Parámetro | Script | Valor por defecto |
|---|---|---|
| `dwellTime` | GazeInteractor | 1.5 s |
| `maxDistance` | GazeInteractor | 30 m |
| `collectDistance` | CollectableItem | 6 m |
| `speed` | CollectableItem | 3 m/s |
| `arriveDistance` | CollectableItem | 1 m |
| `retrieveDwellTime` | CollectableRetriever | 0.4 s |
| `extraHitRadius` | CollectableRetriever | 2 m |
| `cooldown` | CollectableRetriever | 3 s |

> Si en tu escena has cambiado alguno de estos valores, actualiza la tabla.

### Cómo funciona la interacción

```
GazeInteractor (Main Camera)
   └─ Physics.Raycast desde la cámara hacia delante
        └─ collider.GetComponentInParent<GazeTarget>()
             ├─ CollectableItem      → vuela hacia el jugador y se desactiva
             └─ CollectableRetriever → CollectableItem.AllItems.FlyToPlayer(true)
```

Se usa un raycast propio en lugar del sistema de eventos de la UI (`IPointerEnterHandler`), así no hace falta un `EventSystem` ni un `Physics Raycaster`. Todos los objetos interactuables necesitan un **Collider**.

## Jerarquía de la escena

```
Escena
├── Terreno (Terrain)  + decoración (Simple Nature Pack)
├── Monedas (CollectableItem + Collider)
├── Cofre (CollectableRetriever + Collider)
├── Player                     ← CharacterController + PlayerMovement
│   └── Head  (Y = 1.8)
│       └── Main Camera        ← Tracked Pose Driver + GazeInteractor + EditorLook
└── Setup                      ← CardboardStartup
```

## Configuración del proyecto

- **Unity:** 6 (6000.6.3f1)
- **Plugin:** Google Cardboard XR Plugin for Unity **1.36.0**
- **Entrada:** Input System
- **Plataforma:** Android

Ajustes seguidos (según el guion de la asignatura y la guía de Google):

- Módulos de Android instalados (SDK, NDK, OpenJDK) desde Unity Hub.
- XR Plug-in Management → Android → **Cardboard** activado, con **Initialize XR on Startup**.
- Orientación **Landscape Left**.
- Vulkan desactivado en Graphics APIs.
- Scripting Backend **IL2CPP** y arquitectura **ARM64**.
- Internet requerido (`Internet Access: Require`).
- Modificación de las plantillas **Gradle Properties Template** y **Main Gradle Template** (`Assets/Plugins/Android/`).
- Main Camera con el tag `MainCamera` y Near Clipping Plane de 0.01.

Guía de referencia: <https://developers.google.com/cardboard/develop/unity/quickstart?hl=es-419>

## Problemas encontrados

- **`Please initialize Cardboard XR loader before calling this function`:** en `CardboardStartup.cs`, `Api.UpdateScreenParams()` fallaba en el editor, donde el loader no está inicializado. Se rodeó con la directiva de compilación:

  ```csharp
  #if !UNITY_EDITOR
      Api.UpdateScreenParams();
  #endif
  ```

- **Dos sistemas de mirada mezclados:** `CollectableItem` y `CollectableRetriever` estaban hechos con `IPointerEnterHandler`, mientras que `GazeInteractor` solo detecta clases que heredan de `GazeTarget`. Se reescribieron para heredar de `GazeTarget` y usar un único sistema.
- **Retriever difícil de activar:** con el tiempo de mirada global era incómodo. Se añadió un tiempo propio más corto, una zona de detección ampliada y un enfriamiento para que no reinicie las monedas continuamente.
- **Conflicto de scripts del sample:** `VrModeController` y `CardboardStartup` inicializan XR los dos, por lo que solo se usa `CardboardStartup`.
- _(Añade aquí cualquier otro problema, por ejemplo de Gradle, SDK/NDK o de macOS.)_
