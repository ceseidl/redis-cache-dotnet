using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace RedisCache;

// Cache-aside: o serviço consulta o Redis antes do banco.
public class ServicoDeUsuarios(
    IDistributedCache cache,
    BancoDeUsuarios banco,
    TimeSpan ttl)
{
    static string Chave(int id) => $"usuario:{id}";

    public Task<Usuario?> ObterSemCacheAsync(int id) =>
        banco.BuscarAsync(id);

    public async Task<Usuario?> ObterAsync(int id)
    {
        var json = await cache.GetStringAsync(Chave(id));
        if (json is not null)
            return JsonSerializer.Deserialize<Usuario>(json);

        var usuario = await banco.BuscarAsync(id);
        if (usuario is null)
            return null;

        await cache.SetStringAsync(
            Chave(id),
            JsonSerializer.Serialize(usuario),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl,
            });
        return usuario;
    }

    // Chamar sempre que o usuário mudar no banco.
    public Task InvalidarAsync(int id) =>
        cache.RemoveAsync(Chave(id));

    public Task<string?> LerBrutoAsync(int id) =>
        cache.GetStringAsync(Chave(id));
}
