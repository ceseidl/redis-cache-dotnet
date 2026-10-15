# Redis Cache in a .NET Microservice

English | [Português](README.pt-BR.md)

[![CI](https://github.com/ceseidl/redis-cache-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/ceseidl/redis-cache-dotnet/actions/workflows/ci.yml) [![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

> **Quick start**

```bash
docker compose up -d
dotnet run --project src/RedisCache
```

Needs the .NET 10 SDK and a Redis on `localhost:6379`. Details in [Run](#run).

A runnable .NET 10 example of the **cache-aside** pattern with Redis, in the shape of an authentication service that looks up user data. A simulated database (50 ms per query, queries counted) sits behind `IDistributedCache` backed by Redis, and a console demo shows what the cache changes: number of queries, expiration, stale data and what is actually stored.

## Projects

```
src/
└── RedisCache/
    ├── Usuario.cs             # cached DTO (no password) and DB record
    ├── BancoDeUsuarios.cs     # simulated database, counts queries
    ├── ServicoDeUsuarios.cs   # cache-aside: Redis first, then database
    ├── Demo.cs                # the five scenarios
    └── Program.cs             # Redis setup and entry point
```

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A Redis server. The easiest way is Docker: `docker compose up -d` (file `compose.yaml`).

## Run

```bash
docker compose up -d
dotnet run --project src/RedisCache
```

To use another Redis, set `REDIS` (default `localhost:6379`).

## Expected output

```
sem cache    100 leituras  100 consultas  ... ms
com cache    100 leituras  1 consulta   ... ms
após o TTL   1 leitura     1 consulta
sem invalidar: papéis = leitor
invalidando:   papéis = admin
no Redis: {"Id":1,"Nome":"ana","Papeis":["admin"]}
```

- `sem cache` / `com cache`: 100 reads of the same user. Query counts are exact; the times depend on the machine.
- `após o TTL`: after the entry expires (2 s), the next read goes back to the database.
- `sem invalidar`: the database changed the role and the cache still returns the old one until it expires.
- `invalidando`: removing the key after the change makes the next read fresh.
- `no Redis`: the stored JSON has no password hash.

## Things worth knowing

- **Cache-aside is not transactional.** Between the database update and the invalidation a reader can still get the old value. Keep the TTL short for data that drives authorization.
- **Never cache secrets.** The cached `Usuario` has no password hash, token or session data.
- **Redis is a performance layer, not the source of truth.** If it is down, the service should still work, only slower (not covered by this demo).

## Links

- [Distributed caching in ASP.NET Core (Microsoft Learn)](https://learn.microsoft.com/aspnet/core/performance/caching/distributed)
- [Redis documentation](https://redis.io/docs/latest/)

## License

[MIT](LICENSE). Author: Carlos Eduardo Seidl.
