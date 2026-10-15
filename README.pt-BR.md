# Cache Redis em um Microsserviço .NET

[English](README.md) | Português

[![CI](https://github.com/ceseidl/redis-cache-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/ceseidl/redis-cache-dotnet/actions/workflows/ci.yml) [![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

> **Início rápido**

```bash
docker compose up -d
dotnet run --project src/RedisCache
```

Precisa do SDK do .NET 10 e de um Redis em `localhost:6379`. Detalhes em [Como rodar](#como-rodar).

Exemplo executável em .NET 10 do padrão **cache-aside** com Redis, no formato de um serviço de autenticação que busca dados do usuário. Um banco simulado (50 ms por consulta, consultas contadas) fica atrás de um `IDistributedCache` com Redis, e uma demonstração no console mostra o que o cache muda: número de consultas, expiração, dado desatualizado e o que realmente é guardado.

## Projetos

```
src/
└── RedisCache/
    ├── Usuario.cs             # DTO do cache (sem senha) e registro do banco
    ├── BancoDeUsuarios.cs     # banco simulado, conta as consultas
    ├── ServicoDeUsuarios.cs   # cache-aside: Redis primeiro, depois o banco
    ├── Demo.cs                # os cinco cenários
    └── Program.cs             # configuração do Redis e ponto de entrada
```

## Requisitos

- [SDK do .NET 10](https://dotnet.microsoft.com/download)
- Um servidor Redis. O mais simples é o Docker: `docker compose up -d` (arquivo `compose.yaml`).

## Como rodar

```bash
docker compose up -d
dotnet run --project src/RedisCache
```

Para usar outro Redis, defina `REDIS` (padrão `localhost:6379`).

## Saída esperada

```
sem cache    100 leituras  100 consultas  ... ms
com cache    100 leituras  1 consulta   ... ms
após o TTL   1 leitura     1 consulta
sem invalidar: papéis = leitor
invalidando:   papéis = admin
no Redis: {"Id":1,"Nome":"ana","Papeis":["admin"]}
```

- `sem cache` / `com cache`: 100 leituras do mesmo usuário. A contagem de consultas é exata; os tempos dependem da máquina.
- `após o TTL`: depois que a entrada expira (2 s), a leitura seguinte volta ao banco.
- `sem invalidar`: o banco mudou o papel e o cache ainda devolve o antigo até expirar.
- `invalidando`: remover a chave depois da alteração faz a próxima leitura vir atualizada.
- `no Redis`: o JSON guardado não tem o hash da senha.

## O que vale saber

- **Cache-aside não é transacional.** Entre a atualização do banco e a invalidação, um leitor ainda pode receber o valor antigo. Use TTL curto para dados que alimentam autorização.
- **Nunca guarde segredos no cache.** O `Usuario` em cache não tem hash de senha, token nem dados de sessão.
- **O Redis é camada de desempenho, não fonte da verdade.** Se ele cair, o serviço deve continuar funcionando, só mais lento (não coberto por esta demonstração).

## Links

- [Cache distribuído no ASP.NET Core (Microsoft Learn)](https://learn.microsoft.com/aspnet/core/performance/caching/distributed)
- [Documentação do Redis](https://redis.io/docs/latest/)

## Licença

[MIT](LICENSE). Autor: Carlos Eduardo Seidl.
