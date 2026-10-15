using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using RedisCache;

// REDIS = endereço do Redis (padrão: localhost:6379)
var redis = Environment.GetEnvironmentVariable("REDIS")
    ?? "localhost:6379";

var servicos = new ServiceCollection();
servicos.AddStackExchangeRedisCache(o =>
{
    o.Configuration = $"{redis},abortConnect=false";
    o.InstanceName = "demo:";
});
await using var provedor = servicos.BuildServiceProvider();

var banco = new BancoDeUsuarios();
var servico = new ServicoDeUsuarios(
    provedor.GetRequiredService<IDistributedCache>(),
    banco,
    ttl: TimeSpan.FromSeconds(2));

try
{
    await Demo.ExecutarAsync(servico, banco);
}
catch (StackExchange.Redis.RedisConnectionException)
{
    Console.WriteLine($"Redis indisponível em {redis}.");
    Environment.ExitCode = 1;
}
