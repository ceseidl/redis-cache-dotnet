namespace RedisCache;

// O que vai para o cache: sem senha, sem token.
public record Usuario(int Id, string Nome, string[] Papeis);

// O que existe no banco: inclui o hash da senha.
public record UsuarioRegistro(
    int Id, string Nome, string SenhaHash, string[] Papeis);
