using System.Diagnostics;

namespace RedisCache;

public static class Demo
{
    const int Leituras = 100;

    public static async Task ExecutarAsync(
        ServicoDeUsuarios servico, BancoDeUsuarios banco)
    {
        await servico.InvalidarAsync(1);

        // 1. sem cache: toda leitura vai ao banco
        var antes = banco.Consultas;
        var tempo = await Medir(
            () => servico.ObterSemCacheAsync(1));
        Console.WriteLine(
            $"sem cache    {Leituras} leituras  " +
            $"{banco.Consultas - antes} consultas  " +
            $"{tempo} ms");

        // 2. com cache: só a primeira vai ao banco
        antes = banco.Consultas;
        tempo = await Medir(() => servico.ObterAsync(1));
        Console.WriteLine(
            $"com cache    {Leituras} leituras  " +
            $"{banco.Consultas - antes} consulta   {tempo} ms");

        // 3. expiração: depois do TTL, volta ao banco
        await Task.Delay(2500);
        antes = banco.Consultas;
        await servico.ObterAsync(1);
        Console.WriteLine(
            "após o TTL   1 leitura     " +
            $"{banco.Consultas - antes} consulta");

        // 4. dado velho: o banco mudou e o cache não soube
        banco.AlterarPapeis(1, "admin");
        var velho = await servico.ObterAsync(1);
        Console.WriteLine("sem invalidar: papéis = " +
            string.Join(",", velho!.Papeis));

        await servico.InvalidarAsync(1);
        var novo = await servico.ObterAsync(1);
        Console.WriteLine("invalidando:   papéis = " +
            string.Join(",", novo!.Papeis));

        // 5. o que fica guardado no Redis
        Console.WriteLine(
            $"no Redis: {await servico.LerBrutoAsync(1)}");
    }

    static async Task<long> Medir(Func<Task> leitura)
    {
        var relogio = Stopwatch.StartNew();
        for (var i = 0; i < Leituras; i++)
            await leitura();
        return relogio.ElapsedMilliseconds;
    }
}
