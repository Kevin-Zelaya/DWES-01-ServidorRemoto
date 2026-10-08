# Repositorio Remoto

## Objetivo

Desarrollar un servicio en **.NET** que gestione datos con **tres niveles de almacenamiento**: caché (MemoryCache), base de datos local (EF Core + SQLite) y API REST remota. El servicio realizará operaciones CRUD de forma asíncrona y usará las tecnologías vistas en la UD01.

Debes tener en cuenta que cada 60 segundos se sincronizará la base de datos local con la API REST remota, y que al arrancar la aplicación se borrará la base de datos local y se cargará desde la API REST.

Además tendrá un servicio de notificaciones. Para ello usaremos programación reactiva. En Program.cs, nada más arrancar el servicio, se suscribirá a los eventos de creación, actualización y eliminación de usuarios y mostrará un mensaje en consola. Este servicio estará inyectado en el `UserService` y se llamará cada vez que se cree, actualice o elimine un usuario.

Todo tendrá que estar documentado con XMLDoc y tener tests unitarios con NUnit + Moq + FluentAssertions.

---

## Tecnologías utilizadas

| Tecnología | Uso |
|---|---|
| **.NET 10** | Framework principal de la aplicación |
| **ASP.NET Core** | Desarrollo de la API REST |
| **Entity Framework Core** | Acceso y persistencia de datos |
| **SQLite** | Base de datos local en Development |
| **PostgreSQL** | Base de datos en Production |
| **Redis** | Sistema de caché en Production |
| **MemoryCache** | Sistema de caché en Development |
| **Refit** | Consumo de la API REST remota |
| **Docker** | Ejecución de PostgreSQL |
| **CSharpFunctionalExtensions** | Gestión de resultados mediante `Result` |
| **Serilog** | Sistema de logs |
| **NUnit** | Framework para tests unitarios |
| **Moq** | Creación de mocks para los tests |
| **FluentAssertions** | Validación de resultados en los tests |
| **Bruno** | Pruebas de los endpoints de la API |

---

## Estructura del proyecto

```text
RepositorioRemoto/
│
├── config/
│   └── AppConfig.cs
│
├── context/
│   └── DbContext.cs
│
├── controllers/
│   └── UserController.cs
│
├── Dependencies/
│   ├── DependencyProvider.cs
│   └── Lifetimes/
│       ├── Scoped.cs
│       ├── Singleton.cs
│       └── Transient.cs
│
├── dto/
│   ├── CreateUserRequest.cs
│   ├── UpdateUserRequest.cs
│   └── UserDto.cs
│
├── Entities/
│   └── UserEntity.cs
│
├── errors/
│   ├── ApiError.cs
│   ├── CacheError.cs
│   ├── DatabaseError.cs
│   └── DomainError.cs
│
├── mappers/
│   └── UserMapper.cs
│
├── model/
│   └── UserModel.cs
│
├── postgres/
│   ├── Docker-compose.yml
│   └── init.sql
│
├── repository/
│   ├── common/
│   │   ├── IRepository.cs
│   │   ├── IUnitOfWork.cs
│   │   └── UnitOfWork.cs
│   │
│   ├── postgres/
│   │   └── PosgreSqlRepository.cs
│   │
│   └── sqlite/
│       └── SqliteRepository.cs
│
├── service/
│   ├── api/
│   │   ├── IUserApi.cs
│   │   ├── IUserApiService.cs
│   │   └── UserApiService.cs
│   │
│   ├── Background/
│   │   ├── event/
│   │   │   └── DatabaseRefreshedEventArgs.cs
│   │   └── UserSyncBackgroundService.cs
│   │
│   ├── Cache/
│   │   ├── common/
│   │   │   └── ICache.cs
│   │   ├── InMemoryCache.cs
│   │   └── redis/
│   │       └── RedisCache.cs
│   │
│   └── users/
│       ├── iUserService.cs
│       └── UsersService.cs
│
├── bruno/
│   └── RepositorioRemoto-API/
│       ├── Create User.yml
│       ├── Delete User.yml
│       ├── Get All Users.yml
│       ├── Get User By ID.yml
│       ├── Update User.yml
│       └── opencollection.yml
│
├── app.db
├── appsettings.json
├── appsettings.development.json
├── appsettings.production.json
├── Program.cs
└── RepositorioRemoto.csproj
```

---

## Almacenamiento

El proyecto permite trabajar con diferentes implementaciones de almacenamiento, de forma que se puede cambiar entre ellas sin modificar la lógica de negocio.

### Base de datos

Se dispone de dos implementaciones:

- **SQLite**: base de datos local.
- **PostgreSQL**: base de datos ejecutada mediante Docker.

La implementación utilizada se selecciona mediante la configuración de dependencias.

### Caché

También se dispone de dos implementaciones:

- **MemoryCache**: caché almacenada en memoria.
- **Redis**: caché externa mediante un servidor Redis.

Esto permite probar y utilizar diferentes sistemas de almacenamiento manteniendo la misma interfaz.

---

## Configuración y entornos

La aplicación permite cambiar entre diferentes implementaciones de persistencia y caché dependiendo del entorno en el que se ejecute.

La configuración se determina mediante la variable de entorno `DOTNET_ENVIRONMENT`.

### Development

En el entorno `Development` se utilizan:

- **SQLite** como base de datos local.
- **InMemoryCache** como sistema de caché.

```text
Development
├── SQLite
└── InMemoryCache
```

### Production

En el entorno `Production` se utilizan:

- **PostgreSQL** como base de datos.
- **Redis** como sistema de caché.

```text
Production
├── PostgreSQL
└── Redis
```

---

## Funcionamiento de la aplicación

La aplicación utiliza diferentes capas para separar las responsabilidades y permitir cambiar las implementaciones de almacenamiento sin modificar la lógica de negocio.

```text
                         ┌─────────────────┐
                         │     API REST    │
                         │  JSONPlaceholder│
                         └────────▲────────┘
                                  │
                                Refit
                                  │
┌──────────────┐            ┌─────┴────────────┐
│  Controller  │──DTO──────▶│   User Service   │
└──────────────┘            │   (orquestador)   │
                            └─────┬──────┬──────┘
                                  │      │
                            Cache │      │ Repository
                                  ▼      ▼
                         ┌──────────┐  ┌──────────────┐
                         │  Cache   │  │ Base de datos│
                         └──────────┘  └──────┬───────┘
                                              │
                                      ┌───────┴────────┐
                                      │                │
                                   SQLite         PostgreSQL
                                Development       Production
```

El userservice actua como punto central de la lógica de negocio y coordena las operaciones entre la Api remota, la base de datos y la caché.

---

## Sincronización de usuarios

Para la sincronización se usa un Backgroundservice encargado de sincronizar periodicamente los usuarios de la api rest remota con la base de datos local.

### La sincronización se realiza:

- Al iniciar la aplicación.
- Cada 60 segundos.

En este proceso se obtienen los usuarios de la Api remota y se actualiza la base de datos local.

```text
API REST
   │
   │ GET /users
   ▼
UserApiService
   │
   ▼
UserService
   │
   ├──► SQLite / PostgreSQL
   │
   └──► Cache
```

---

## Servicio de notificaciones

La aplicación dispone de un servicio de notificación basado en enventos para aplicar programación reactiva.

El servicio permite notificar las operaciones realizadas sobre los usuarios:

- Crear usuario.
- Actualizar usuario.
- Eliminar usuario.

El INotificationService se inyecta en el userservice y se utiliza cada vez que se realiza una de las anteriores operaciones.

```text
UserService ── UserModel ──▶ INoificationService ── UserModel ──▶ Program
```

---

## API REST

La aplicación despliega una Api Rest para gestionar los usuarios.

### Endpoints

| Método | Endpoint | Descripción |
|---|---|---|
| `GET` | `/api/users` | Obtener todos los usuarios |
| `GET` | `/api/users/{id}` | Obtener un usuario por ID |
| `POST` | `/api/users` | Crear un nuevo usuario |
| `PUT` | `/api/users/{id}` | Actualizar un usuario |
| `DELETE` | `/api/users/{id}` | Eliminar un usuario |

Este enfoque nos permite delegar la validación de datos, esto mediante el middleware de validación y las anotaciones en los dtos.

```csharp
public class CreateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string name { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string email { get; set; } = string.Empty;

    [Required]
    public AddressDto address { get; set; } = new();

    [Phone]
    [StringLength(100)]
    public string? phone { get; set; }

    [Url]
    [StringLength(100)]
    public string? website { get; set; }

    [Required]
    public CompanyDto company { get; set; } = new();
}
```

---

## Gestion de errores

Por último utilizamos **Result** para la gestión de resultados de las operaciones sin depender directamente de excepciones.

Los errores se encuentran organizados segúin su origen:

```text
DomainError
├── ApiError
├── CacheError
└── DatabaseError
```

```csharp
public record DomainError(
    string message,
    HttpStatusCode StatusCode,
    string? detail = null);
```

```csharp
public abstract record DatabaseError(
    string Message,
    HttpStatusCode StatusCode,
    string? Details = null)
    : DomainError(Message, StatusCode, Details)
{
    public record ConnectionFailure(string Details)
        : DatabaseError(
            "No se pudo establecer la conexión con la base de datos.",
            HttpStatusCode.ServiceUnavailable,
            Details);
}
```