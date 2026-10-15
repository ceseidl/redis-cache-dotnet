namespace RedisCache;

// Banco simulado: cada consulta custa 50 ms e é contada.
public class BancoDeUsuarios
{
    readonly Dictionary<int, UsuarioRegistro> _dados = new()
    {
        [1] = new(1, "ana", "hash-de-exemplo", ["leitor"]),
    };
    int _consultas;

    public int Consultas => _consultas;

    public async Task<Usuario?> BuscarAsync(int id)
    {
        Interlocked.Increment(ref _consultas);
        await Task.Delay(50);
        return _dados.TryGetValue(id, out var r)
            ? new Usuario(r.Id, r.Nome, r.Papeis)
            : null;
    }

    public void AlterarPapeis(int id, params string[] papeis) =>
        _dados[id] = _dados[id] with { Papeis = papeis };
}
