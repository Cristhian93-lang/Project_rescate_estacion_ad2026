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
| `EstacionUnity/` | Cliente en Unity (URP) con la visualización 3D |
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

## Flujo de trabajo (GitFlow)
- `main` solo recibe versiones terminadas por Pull Request.
- Cada iteración tiene su rama `pre-release-x.x.x`.
- Cada funcionalidad tiene su rama `pre-release-x.x.x.n-nombre`, que se une a `pre-release-x.x.x` por Pull Request.
- Todo cambio se anota en el `CHANGELOG.md`.

## Créditos de assets
- [Modular Sci-Fi MegaKit](https://quaternius.com/packs/modularscifimegakit.html), Quaternius (CC0).
