# Diseño de comportamientos de los agentes

**Autor:** Nicolás Bravo Miguel
**Rama:** `pre-release-0.1.0.5-diseno-agentes`
**Fecha:** 18/11/2026

---

## 1. Contexto y objetivo

Los agentes son el **escuadrón** de rescate de la estación. El objetivo del reto es **rescatar 7 pilotos** antes de **perder 4** o de que el casco llegue a **25 de daño** (despresurización).

Vamos a implementar dos estrategias para comparar cuál cumple mejor el objetivo:

- **Aleatoria:** sirve como referencia.
- **Mejorada:** cooperativa, con utilidad, rutas de menor costo, roles y memoria compartida.

---

## 2. Tipo de agente y entorno de trabajo

### 2.1 Tipo de agente (presentación 01)

| Estrategia | Tipo de agente | Por qué |
|---|---|---|
| Aleatoria | **Reactivo simple** | Elige al azar una acción que puede pagar. No recuerda nada ni se coordina, como las barredoras de RobotSweep. |
| Mejorada | **Basado en utilidad** | Calcula una utilidad para cada objetivo posible y elige el mejor, no solo uno que cumpla una meta. |

### 2.2 Rendimiento, entorno, actuadores y sensores

| Tipo de agente | Medidas de rendimiento | Entorno | Actuadores | Sensores |
|---|---|---|---|---|
| Agente del escuadrón | Pilotos rescatados (meta: 7), pilotos perdidos (límite: 4), daño al casco (límite: 25) y PA bien aprovechados | Estación con cuartos, paredes, compuertas, señales, criaturas, esporas y hangar. Cambia al azar al terminar cada turno | Moverse, abrir y cerrar compuertas, limpiar esporas, eliminar criaturas, golpear paredes, cargar pilotos | Su celda, las celdas vecinas y la memoria compartida |

### 2.3 Propiedades del entorno (presentación 01)

| Propiedad | Clasificación | Justificación |
|---|---|---|
| Observabilidad | **Parcialmente observable** | No se sabe qué es una señal (piloto o falsa alarma) hasta revelarla. |
| Determinismo | **Estocástico** | El avance de las criaturas y el fuego dependen de dados. |
| Episódico / secuencial | **Secuencial** | Una decisión (por ejemplo, gastar PA o dejar una compuerta abierta) afecta las siguientes. |
| Estático / dinámico | **Estático** | La estación solo cambia al terminar cada turno, no mientras el agente decide. |
| Discreto / continuo | **Discreto** | Casillas, turnos y puntos de acción (PA). |
| Individual / multiagente | **Multiagente cooperativo** | Todos los agentes maximizan el mismo rendimiento: rescatar pilotos sin perder la estación. |

---

## 3. Reglas y costos de PA

| Acción | Costo |
|---|---|
| Moverse | 1 PA |
| Moverse cargando a un piloto | 2 PA. No puede entrar a una casilla con criatura |
| Compuerta cerrada (abrirla para pasar) | +1 PA |
| Limpiar esporas | 1 PA |
| Cerrar una compuerta | 1 PA |
| Convertir una criatura en esporas | 1 PA |
| Eliminar una criatura completa | 2 PA |
| Golpear una pared | 2 PA y +1 de daño al casco |
| Romper una pared | 2 golpes: 4 PA y 2 de daño |
| Revelar una señal | Gratis al entrar a su casilla |
| Guardar PA | Máximo 8 |

Las señales se **reponen a 3** al final de cada turno. Se pierde con **25 de daño o más** al casco.

---

## 4. Estrategias

### 4.1 Estrategia aleatoria (reactivo simple)

En su turno, cada agente revisa qué acciones puede pagar con sus PA: moverse, abrir o cerrar una compuerta, limpiar esporas, convertir una criatura en esporas, eliminar una criatura, golpear una pared, cargar a un piloto o terminar el turno. Elige una al azar y repite hasta gastar sus PA o hasta que elija terminar el turno. No recuerda nada ni se coordina con los demás.

### 4.2 Estrategia mejorada (basado en utilidad)

Cada agente tiene un rol y cada rol tiene sus propios comportamientos.

**Rescatador (unos 4 agentes)**

- **Buscar:** elige la señal más conveniente que ningún otro agente haya reclamado.
- **Ir a la señal:** se mueve por la ruta que le cuesta menos.
- **Revelar:** al llegar, descubre si la señal es un piloto o una falsa alarma.
- **Evacuar:** si es un piloto, lo carga y lo lleva al hangar más cercano.
- **Abrirse paso:** si una criatura o esporas bloquean la ruta, las elimina. Solo golpea una pared si no hay otra ruta.

**Guardián (unos 2 agentes)**

- **Vigilar:** revisa el mapa de amenazas para encontrar el foco más peligroso.
- **Contener:** va al foco y elimina las criaturas o las esporas.
- **Proteger:** atiende primero las criaturas que están junto a pilotos o señales, y las esporas que están junto a una criatura.
- **Cuidar el casco:** atiende los focos junto a paredes dañadas para evitar eclosiones.
- **Cerrar compuertas (1 PA):** cierra compuertas para que las criaturas no se pasen a otro cuarto.

**Ambos roles**

- **Cambiar de rol:** si un guardián está junto a una señal libre y no hay rescatador cerca, la toma él.
- **Guardar PA:** si la siguiente acción útil cuesta más de lo que le queda, termina su turno y guarda los PA (máximo 8) para el siguiente.
- **Evitar el peligro al terminar el turno:** no termina su turno junto a criaturas para no quedar derribado.

---

## 5. Cómo decide el agente

### 5.1 Ciclo de decisión

En cada acción, el agente de la estrategia mejorada sigue el mismo ciclo:

1. **Percibir:** lee su celda, las celdas vecinas y la memoria compartida.
2. **Elegir objetivo:** a cada objetivo posible le calcula una utilidad, `utilidad = valor × urgencia / costo en PA`, y elige el de mayor utilidad.
3. **Planear la ruta:** calcula con Dijkstra el camino de menor costo.
4. **Actuar:** hace solo la primera acción de esa ruta.
5. **Volver a empezar:** como la estación cambia al azar después de cada turno, recalcula antes de la siguiente acción.

### 5.2 Valor de cada objetivo (de mayor a menor)

1. Llevar al hangar al piloto que ya carga.
2. Eliminar una criatura que está junto a un piloto o una señal, porque se puede perder.
3. Limpiar esporas junto a una criatura, porque en el siguiente avance se vuelven criatura.
4. Rescatar una señal.
5. Atender criaturas o esporas junto a paredes dañadas, por el riesgo de eclosión.

### 5.3 Dijkstra con costo en PA

Usamos el algoritmo de Dijkstra, con una fila de prioridades. El costo de cada paso es:

| Paso | Costo |
|---|---|
| Mover | 1 (2 si carga un piloto) |
| Compuerta cerrada | +1 |
| Casilla con criatura | +2 (si carga un piloto, no se puede entrar) |
| Pared | 4 + penalización por daño al casco |

La penalización por daño de la pared crece conforme el daño acumulado se acerca a 25. Así el agente rompe una pared solo cuando no hay otra ruta razonable.

### 5.4 Innovación: ruta con riesgo

Además del costo en PA, el costo de cada casilla en Dijkstra suma su **riesgo**. Las casillas junto a criaturas tienen más probabilidad de recibir fuego en el siguiente avance.

```
costo_total(casilla) = costo_PA(casilla) + λ · riesgo(casilla)
riesgo(casilla)      = número de criaturas en las casillas vecinas
```

Así el agente no solo busca la ruta más corta, sino la más **segura**. Es la misma idea de agente basado en utilidad. El peso `λ` se ajusta con `batch_run`. Para decidir si le alcanzan los PA, el agente usa siempre el costo real en PA, sin el riesgo.

---

## 6. Cómo interactúan los agentes

### 6.1 Memoria compartida

Los agentes no se mandan mensajes directos: se coordinan con una **memoria compartida** guardada en el modelo. Es el modo de *compartir resultados* de la presentación 02, igual que `visited_cells` en RobotSweep. Ahí escriben:

- las señales conocidas y quién reclamó cada una,
- el mapa de amenazas (criaturas y esporas),
- las tareas abiertas.

### 6.2 Red de contratos

Las tareas nuevas se reparten con la red de contratos, usando la memoria compartida como medio de comunicación:

| Etapa | Qué pasa en nuestro sistema |
|---|---|
| **Reconocimiento** | El agente que descubre un piloto, una señal o un foco de criaturas reconoce que hay una tarea que conviene repartir. Ese agente es el **gerente**. |
| **Anuncio** | El gerente escribe la tarea en la memoria compartida: qué es, en qué casilla está y qué restricciones tiene (por ejemplo, requiere cargar). |
| **Oferta** | Cada agente libre calcula con Dijkstra cuánto le costaría en PA y ofrece ese costo. Si está ocupado o no le alcanzan los PA, rechaza. |
| **Adjudicación** | El gerente adjudica la tarea al agente con la oferta más barata. Los demás la ignoran. |
| **Ejecución** | El contratista hace la tarea y avisa al gerente escribiendo el resultado en la memoria. |

Reglas complementarias:

- **Reclamar antes de ir:** el agente marca la señal o el foco como suyo. Así dos agentes nunca van al mismo objetivo.
- **Avisar:** quien descubre un piloto o un foco junto a una señal lo escribe en la memoria para que los demás reaccionen.
- **Liberar:** si un objetivo se vuelve demasiado caro porque la criatura se propagó, el agente lo suelta y otro puede tomarlo.
- **Complementarse:** los rescatadores evacúan mientras los guardianes despejan las rutas y protegen las señales.

### 6.3 Coaliciones

Los roles se pueden ver como coaliciones: con 6 agentes, la estructura de coaliciones es {{rescatadores}, {guardianes}}. Son subconjuntos disjuntos cuya unión es el escuadrón completo. Cuando un guardián cambia de rol, la estructura de coaliciones cambia.

### 6.4 Ejemplo

Aparece una señal en (3, 5) con una criatura al lado. El agente que la descubre actúa como gerente y anuncia dos tareas: rescatar en (3, 5) y contener la criatura. El rescatador más barato gana la señal y el guardián más barato gana la criatura. El guardián llega primero y elimina la criatura. El rescatador revela la señal, encuentra un piloto y lo evacúa al hangar por la ruta que el guardián despejó.

---

## 7. Cómo apoyan el objetivo del reto

| Condición del juego | Comportamientos que la atienden |
|---|---|
| Rescatar 7 pilotos | Hay rescatadores dedicados a eso. Como usan la ruta de menor costo en PA, alcanzan más señales por turno. |
| No perder 4 pilotos | Los guardianes atienden primero las criaturas junto a pilotos y señales, y cierran compuertas para contenerlas. Evitar terminar el turno junto a criaturas reduce los derribos. |
| No llegar a 25 de daño | Los guardianes atienden los focos junto a paredes dañadas, y las rutas evitan romper paredes. |
| No desperdiciar PA | Reclamar objetivos evita que dos agentes hagan el mismo trabajo, y guardar PA evita gastarlos en acciones inútiles. |

### 7.1 Cómo contribuye cada algoritmo a los objetivos del sistema

| Algoritmo | Objetivos que apoya | Cómo contribuye |
|---|---|---|
| Elección aleatoria (estrategia aleatoria) | Todos, como referencia | No busca cumplir los objetivos: sirve como **línea base**. Comparar contra ella con `batch_run` muestra cuánto mejora la estrategia mejorada. |
| Función de utilidad (`valor × urgencia / costo en PA`) | Rescatar 7, no perder 4, no llegar a 25 de daño | Ordena los objetivos por importancia y urgencia. Así el agente atiende primero lo que más se puede perder (un piloto cargado o una criatura junto a una señal) y no gasta PA en objetivos de poco valor. |
| Dijkstra con costo en PA | Rescatar 7, no desperdiciar PA | Encuentra la ruta más barata a cada objetivo. Con menos PA por viaje, el escuadrón alcanza más señales y evacúa más pilotos por turno. |
| Penalización por daño en paredes | No llegar a 25 de daño | Hace que romper una pared sea cada vez más caro conforme el casco se acerca a 25. El agente solo la rompe cuando no hay otra ruta razonable. |
| Ruta con riesgo (innovación) | No perder 4 | Evita las casillas junto a criaturas, que tienen más probabilidad de recibir fuego. Reduce los derribos y protege al piloto que se carga. |
| Memoria compartida | No desperdiciar PA, no perder 4 | Todos conocen las señales, las amenazas y las tareas abiertas sin verlas directamente. Evita trabajo repetido y permite reaccionar rápido cuando aparece un foco junto a una señal. |
| Red de contratos | Rescatar 7, no desperdiciar PA | Asigna cada tarea al agente que la hace con menos PA. Ningún objetivo se queda sin atender y dos agentes nunca van al mismo. |
| Coaliciones (roles) | Rescatar 7, no perder 4, no llegar a 25 de daño | Divide el trabajo: los rescatadores se enfocan en sacar pilotos y los guardianes en contener criaturas y cuidar el casco. Cambiar de rol permite aprovechar una señal libre cuando no hay rescatador cerca. |

---

## 8. Plan de implementación y evaluación

Se implementa por partes para ver la mejora paso a paso:

- **Paso 1: estrategia aleatoria.**
- **Paso 2: mejorada básica,** con utilidad + Dijkstra y sin roles.
- **Paso 3: mejorada completa,** con roles + red de contratos + memoria compartida + riesgo en las rutas.

En cada paso se corren **100 partidas** con `batch_run` y se compara:

- porcentaje de victorias,
- pilotos rescatados,
- pilotos perdidos,
- daño promedio al casco.

Con eso tenemos los datos comparativos que pide la rúbrica y una mejora que se ve paso a paso.

También con `batch_run` se prueba **cuántos rescatadores conviene** (4/2, 3/3 y 5/1 rescatadores/guardianes) y nos quedamos con la combinación que gane más. Así la estrategia queda "mejorada con datos".


