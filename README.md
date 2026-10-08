# Repositorio Remoto

## Objetivo

Desarrollar un servicio en **.NET** que gestione datos con **tres niveles de almacenamiento**: caché (MemoryCache), base de datos local (EF Core + SQLite) y API REST remota. El servicio realizará operaciones CRUD de forma asíncrona y usará las tecnologías vistas en la UD01.

Debes tener en cuenta que cada 60 segundos se sincronizará la base de datos local con la API REST remota, y que al arrancar la aplicación se borrará la base de datos local y se cargará desde la API REST.

Además tendrá un servicio de notificaciones. Para ello usaremos programación reactiva. En Program.cs, nada más arrancar el servicio, se suscribirá a los eventos de creación, actualización y eliminación de usuarios y mostrará un mensaje en consola. Este servicio estará inyectado en el `UserService` y se llamará cada vez que se cree, actualice o elimine un usuario.

Todo tendrá que estar documentado con XMLDoc y tener tests unitarios con NUnit + Moq + FluentAssertions.

---


## Estructura del proyecto

```text
RepositorioRemoto/
│
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


### Almacenamiento

El proyecto permite trabajar con diferentes implementaciones de almacenamiento,
de forma que se puede cambiar entre ellas sin modificar la lógica de negocio.

#### Base de datos

Se dispone de dos implementaciones:

- **SQLite**: base de datos local.
- **PostgreSQL**: base de datos ejecutada mediante Docker.

La implementación utilizada se selecciona mediante la configuración de
dependencias.

#### Caché

También se dispone de dos implementaciones:

- **MemoryCache**: caché almacenada en memoria.
- **Redis**: caché externa mediante un servidor Redis.

Esto permite probar y utilizar diferentes sistemas de almacenamiento
manteniendo la misma interfaz.

## Configuración y entornos

La aplicación permite cambiar entre diferentes implementaciones de
persistencia y caché dependiendo del entorno en el que se ejecute.

La configuración se determina mediante la variable de entorno
`DOTNET_ENVIRONMENT`.

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