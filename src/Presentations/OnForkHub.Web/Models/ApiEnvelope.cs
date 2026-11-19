namespace OnForkHub.Web.Models;

/// <summary>
/// Envelope de resposta padrão das APIs REST do OnForkHub.
/// </summary>
/// <typeparam name="T">Tipo do conteúdo retornado em Data.</typeparam>
public sealed class ApiEnvelope<T>
{
    /// <summary>
    /// Dados retornados pela operação.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Mensagem informativa da resposta.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Lista de erros gerais, se houver.
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Identificador de rastreamento da requisição/resposta.
    /// </summary>
    public string? Id { get; set; }
}
