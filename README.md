# Nightwatch Protocol

https://github.com/JuanDeRe/NP-AuthService/actions/workflows/ci.yml/badge.svg](https://github.com/JuanDeRe/NP-AuthService/actions/workflows/ci.yml)

**Nightwatch Protocol** es un proyecto académico para la materia de Arquitecturas de Software. La aplicación consiste en un juego multijugador de terror asimétrico en tiempo real, donde un jugador toma el rol de vigilante nocturno y otros jugadores controlan animatrónicos que intentan llegar a la oficina y ejecutar un ataque exitoso.

El proyecto busca demostrar una arquitectura distribuida, orientada a concurrencia, sincronización en tiempo real, escalabilidad y observabilidad.

## Arquitectura general

La solución está planteada como una arquitectura distribuida, cliente-servidor y basada en eventos.

Componentes principales:

- **Cliente Unity/C#**: renderizado del juego, UI, input, audio e interpolación.
- **Auth Service**: autenticación básica, generación de JWT y manejo de roles.
- **Matchmaking Service**: cola de jugadores y asignación de partidas.
- **Realtime Gateway**: manejo de conexiones WebSocket para gameplay en tiempo real.
- **Game Service**: estado autoritativo de partidas, validación de acciones y reglas de juego.
- **Observability Service**: monitoreo operativo, métricas, KPIs y dashboard administrativo.
- **Redis / Event Bus**: coordinación temporal, estado de servidores y métricas operativas.
- **PostgreSQL**: persistencia para autenticación y observabilidad.

## Estado actual del desarrollo

Actualmente se está implementando el primer servicio backend:

### Auth Service

Servicio en **C#/.NET** encargado de:

- Realizar login básico.
- Generar tokens JWT.
- Manejar roles de usuario (`player`, `admin`).
- Exponer endpoints REST.
- Preparar la base para integración futura con PostgreSQL.

Por ahora, el repositorio de usuarios es en memoria para validar el flujo inicial. La estructura permite reemplazarlo posteriormente por una implementación con base de datos sin modificar la lógica principal de autenticación.

## Estructura inicial

```text
NightwatchProtocol/
│
├── AuthService/
│   ├── Configuration/
│   ├── Controllers/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   ├── Program.cs
│   └── appsettings.json
│
└── NightwatchProtocol.sln
