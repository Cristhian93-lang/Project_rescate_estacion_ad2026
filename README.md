# Rescate en la Estación

Proyecto de Multiagentes del equipo 7.

Simulación multiagente basada en *Flash Point: Fire Rescue* (reglas de la versión familiar),
con temática de ciencia ficción: un escuadrón de rescate entra a una estación espacial
infestada para sacar a los científicos antes de que el casco se despresurice.

**Materia:** Modelación de sistemas multiagentes con gráficas computacionales (TC2008B)

## Integrantes
- Cristhian Viery Maida Suarez · A01668790
- Gerardo Martinez Carbajal · A01713474
- Nombre Apellido · Matrícula

## Estructura
| Carpeta | Contenido |
|---|---|
| `RescateEstacion/` | Cliente en Unity (URP) con la visualización 3D |
| `servidor/` | Simulación en Python con Mesa |
| `servidor/final.txt` | Configuración inicial del tablero (paredes, señales, aliens, compuertas, entradas) |
| `docs/` | Material de apoyo: plano de la estación, diagramas, presentación |

## Equivalencias con el juego original
| Flash Point | Estación |
|---|---|
| Edificio | Estación espacial |
| Exterior | Hangar |
| Bomberos | Escuadrón de rescate |
| Humo | Esporas |
| Fuego | Alien |
| Explosión | Eclosión |
| Puertas | Compuertas |
| Víctimas | Científicos |
| Falsas alarmas | Señales falsas |
| Puntos de interés | Señales de vida |
| Ambulancia | Enfermería |
| Colapso del edificio | Despresurización |

## Convenciones del equipo
Todo el repositorio se escribe en español: commits, títulos de PR, comentarios de revisión, CHANGELOG y código propio.

### Ramas (GitFlow del Lab 4)
- `main` solo recibe versiones terminadas, por Pull Request. Nadie hace commits directos a `main`.
- Cada iteración tiene su rama `pre-release-x.x.x` (por ejemplo `pre-release-0.1.0`).
- Cada funcionalidad tiene su rama `pre-release-x.x.x.n-nombre`, creada desde `pre-release-x.x.x`.
  - `n` es el número de la funcionalidad dentro de la iteración (1, 2, 3...).
  - `nombre` es una descripción corta en minúsculas con guiones: `pre-release-0.1.0.3-prefabs-estacion`.
- Las funcionalidades se unen a `pre-release-x.x.x` por Pull Request. Cuando la iteración está completa,
  `pre-release-x.x.x` se une a `main` con un PR titulado `Release x.x.x` y se crea el tag `vx.x.x`.

```bash
git checkout pre-release-0.1.0
git pull
git checkout -b pre-release-0.1.0.3-nombre
# ... cambios y commits ...
git push --set-upstream origin pre-release-0.1.0.3-nombre   # solo la primera vez
```

### Commits y títulos de PR (Conventional Commits)
Formato: `tipo: descripción en minúsculas`. El tipo va en inglés porque es el estándar; la descripción va en español.

| Tipo | Cuándo se usa | Ejemplo |
|---|---|---|
| `feat` | Funcionalidad nueva | `feat: graybox de la estación desde final.txt` |
| `fix` | Corrección de un error | `fix: paredes duplicadas en los bordes del tablero` |
| `refactor` | Reorganizar código sin cambiar lo que hace | `refactor: separa la lectura del tablero de la construcción` |
| `docs` | README, CHANGELOG, documentación | `docs: agrega convenciones del equipo` |
| `chore` | Configuración del proyecto, .gitignore, ajustes de Unity | `chore: cambia active input handling a both` |

El título del PR usa el mismo formato que los commits.

### CHANGELOG
- Se agrega **una línea por funcionalidad terminada**, no una por commit, con la fecha al final: `(18/11/2026)`.
- La línea se agrega en la misma rama de la funcionalidad, antes de abrir el PR.
- La sección depende del tipo: `feat` va en **Added**, `refactor` en **Changed**, `fix` en **Fixed**.
- Antes de abrir el PR, hacer `git pull origin pre-release-x.x.x` para traer los cambios del equipo y evitar conflictos.
- Al unir la iteración a `main`, se cambia `UNRELEASED` por la fecha de lanzamiento.

### Unity
- Solo una persona modifica la escena principal (`Assets/Scenes/Estacion`). Los demás trabajan en **prefabs** y los entregan para integrarlos.
- Los assets externos van en `Assets/ThirdParty/` y se registran en **Créditos de assets**.
- Todos usan la misma versión de Unity.

### Revisión de PR
- Cada PR necesita la aprobación de otro integrante antes de hacer merge.
- Los comentarios de revisión usan [Conventional Comments](https://conventionalcomments.org/), en español:
  - `issue:` un problema que hay que corregir.
  - `suggestion:` una propuesta de cambio.
  - `question:` una duda sobre el código.
  - `nitpick:` un detalle menor (formato, typo).
  - `praise:` algo que quedó bien.
- Ejemplo: `issue: si el final.txt tiene una línea vacía al inicio, la lectura falla.`
- Después del merge, borrar la rama.

## Créditos de assets
- [Modular Sci-Fi MegaKit](https://quaternius.com/packs/modularscifimegakit.html), Quaternius (CC0).
