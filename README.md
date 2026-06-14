# WorkDistribution

Sistema de microservicios para repartir ítems de trabajo entre usuarios de forma automática, según la fecha de entrega, la relevancia y la carga que tenga cada uno.

## Con qué está hecho

- .NET 8 (sirve también con .NET Core 3.1 o superior)
- C# con ASP.NET Core Web API
- Swagger para probar los endpoints
- Los datos se guardan en memoria, pero detrás de interfaces, así que cambiar a una base de datos real sería sencillo.

## Cómo está organizado

Son dos microservicios separados, cada uno con sus 3 capas (Controllers, Services, Repositories):

- **UserManagement.API**: maneja los usuarios y cuántas tareas tiene cada uno.
- **WorkItems.API**: maneja los ítems de trabajo y decide a quién asignarlos (acá vive el algoritmo).

Los dos se comunican por HTTP: cuando WorkItems va a repartir un ítem, le pide los usuarios a UserManagement y, después de asignar, le avisa la nueva carga.

## Cómo correrlo

Necesitas tener instalado el SDK de .NET 8.

**1. Configurar el puerto**

WorkItems necesita saber dónde está corriendo UserManagement. Eso se pone en el `appsettings.json` de WorkItems.API:

```json
"Services": {
  "UserManagementApi": "https://localhost:7227/"
}
```

Si en tu pc el microsevicio UserManagement.API corre en otro puerto, solo cambias ese valor. Lo puedes ver en su launchSettings.json o en la URL del navegador cuando abre su Swagger.

**2. Arrancar los dos servicios a la vez**

Como se hablan entre sí, los dos tienen que estar corriendo.

En Visual Studio: clic derecho en la solución → Properties → Multiple startup projects → pon los dos en "Start" y dale play.

O por consola, en dos terminales:

```bash
cd UserManagement.API
dotnet run
```

```bash
cd WorkItems.API
dotnet run
```

**3. Probar**

Cada servicio abre su Swagger en el navegador. Desde ahí puedes probar todo.

## Endpoints

**UserManagement.API**

- `GET /api/users` — lista los usuarios
- `GET /api/users/{username}` — busca un usuario
- `POST /api/users` — crea un usuario
- `PUT /api/users/{username}/workload` — actualiza la carga de un usuario

**WorkItems.API**

- `GET /api/workitems` — lista los ítems
- `GET /api/workitems/user/{username}` — ítems de un usuario
- `POST /api/workitems/distribute` — distribuye los intems de trabajo
- `POST /api/workitems/distribute/batch` — reparte varios ítems a la vez (los más urgentes y relevantes primero)

## Las reglas para repartir

Cuando llega un ítem, se decide a quién dárselo así:

1. Si vence en menos de 3 días, se le da al que tenga menos ítems, sin importar la relevancia.
2. Si un usuario ya tiene más de 3 ítems muy relevantes, se considera saturado y no se le asigna nada.
3. Entre los que quedan, se elige al que tenga menos pendientes.

La relevancia solo puede ser baja (0) o alta (1).

## Decisiones que tomé

El enunciado dejaba algunos casos sin explicar, así que decidí lo siguiente:

- Si dos usuarios tienen la misma cantidad de pendientes, gana el primero por orden alfabético, para que el resultado sea siempre el mismo y no salga al azar.
- Si no hay usuarios o todos están saturados, el ítem no se asigna y la API responde con un 409.
- Para medir quién tiene "menos ítems" uso los pendientes, no los completados (los completados ya no cargan al usuario).
- Cada microservicio tiene sus propios datos y modelos. WorkItems usa un cliente HTTP y sus propios DTOs para hablar con el otro, sin compartir código.
- La dirección del otro servicio va en el appsettings, no quemada en el código, por si los puertos cambian en otra máquina.

## Datos de prueba

Al arrancar UserManagement ya vienen cargados dos usuarios del ejemplo del enunciado, para no tener que crearlos a mano:

| Usuario | Pendientes | Completados | Muy relevantes |
|---------|-----------|-------------|----------------|
| userA | 3 | 0 | 2 |
| userB | 1 | 0 | 0 |

## Ejemplo

Mandando este ítem (muy relevante, vence en 2 días) a `POST /api/workitems/distribute`:

```json
{
  "title": "Tarea urgente del ejemplo",
  "dueDate": "2026-06-16",
  "relevance": 1
}
```
Para repartir varios de golpe, se manda una lista a `POST /api/workitems/distribute/batch`:

```[
  { "title": "Item poco relevante", "dueDate": "2026-08-01", "relevance": 0 },
  { "title": "Item muy relevante", "dueDate": "2026-08-01", "relevance": 1 },
  { "title": "Item que vence pronto", "dueDate": "2026-06-15", "relevance": 0 }
]
```
Se reparten todos, procesando primero el que vence pronto, luego el muy relevante y al final el poco relevante.

