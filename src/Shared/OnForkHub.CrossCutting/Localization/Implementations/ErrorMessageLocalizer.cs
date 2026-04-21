namespace OnForkHub.CrossCutting.Localization.Implementations;

using System.Globalization;

/// <summary>
/// Default implementation of error message localizer.
/// Supports English (en) and Portuguese (pt-BR) by default.
/// </summary>
public class ErrorMessageLocalizer : IErrorMessageLocalizer
{
    private const string DefaultLanguage = "en";
    private readonly Dictionary<string, Dictionary<string, string>> _messages;
    private readonly object _lockObject = new();

    public ErrorMessageLocalizer()
    {
        _messages = new Dictionary<string, Dictionary<string, string>>();
        InitializeDefaultMessages();
    }

    /// <summary>
    /// Gets a localized error message by error code and language.
    /// </summary>
    /// <returns></returns>
    public string GetMessage(string errorCode, string languageCode = "en", params object[]? parameters)
    {
        ArgumentNullException.ThrowIfNull(errorCode);

        lock (_lockObject)
        {
            var language = NormalizeLanguageCode(languageCode);

            if (!_messages.TryGetValue(language, out var msgs))
            {
                language = DefaultLanguage;
                msgs = _messages[language];
            }

            if (!msgs.TryGetValue(errorCode, out var template))
            {
                return $"Error: {errorCode}";
            }

            if (parameters == null || parameters.Length == 0)
            {
                return template;
            }

            try
            {
                return string.Format(CultureInfo.InvariantCulture, template, parameters);
            }
            catch (FormatException)
            {
                return template;
            }
        }
    }

    /// <summary>
    /// Gets a validation error message for a specific property.
    /// </summary>
    /// <returns></returns>
    public string GetValidationMessage(string propertyName, string errorType, string languageCode = "en", params object[]? parameters)
    {
        ArgumentNullException.ThrowIfNull(propertyName);
        ArgumentNullException.ThrowIfNull(errorType);

        var key = $"VALIDATION_{errorType.ToUpperInvariant()}";
        var parmsList = new List<object?> { propertyName };
        if (parameters != null)
        {
            parmsList.AddRange(parameters);
        }

        return GetMessage(key, languageCode, parmsList.Cast<object>().ToArray());
    }

    /// <summary>
    /// Gets all available languages.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<string> GetSupportedLanguages()
    {
        lock (_lockObject)
        {
            return _messages.Keys.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Registers a custom error message.
    /// </summary>
    public void RegisterMessage(string errorCode, string languageCode, string message)
    {
        ArgumentNullException.ThrowIfNull(errorCode);
        ArgumentNullException.ThrowIfNull(message);

        lock (_lockObject)
        {
            var language = NormalizeLanguageCode(languageCode);

            if (!_messages.TryGetValue(language, out var msgs))
            {
                msgs = new Dictionary<string, string>();
                _messages[language] = msgs;
            }

            msgs[errorCode] = message;
        }
    }

    private static string NormalizeLanguageCode(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return DefaultLanguage;
        }

        return languageCode.ToLowerInvariant() switch
        {
            "pt" or "pt-br" or "pt-pt" => "pt-BR",
            _ => languageCode.ToLowerInvariant(),
        };
    }

    private void InitializeDefaultMessages()
    {
        var enMessages = new Dictionary<string, string>
        {
            { "PROPERTY_REQUIRED", "{0} is required" },
            { "INVALID_FORMAT", "{0} has an invalid format" },
            { "INVALID_LENGTH", "{0} must have between {1} and {2} characters" },
            { "INVALID_EMAIL", "{0} must be a valid email address" },
            { "INVALID_URL", "{0} must be a valid URL" },
            { "VALUE_NOT_FOUND", "No record found with the provided value" },
            { "CONFLICT_ERROR", "A record with this value already exists" },
            { "UNAUTHORIZED", "You do not have permission to perform this action" },
            { "FORBIDDEN", "Access to this resource is forbidden" },
            { "INTERNAL_ERROR", "An internal server error occurred" },
            { "VALIDATION_REQUIRED", "{0} is required" },
            { "VALIDATION_INVALIDFORMAT", "{0} has an invalid format" },
            { "VALIDATION_LENGTH", "{0} must have between {1} and {2} characters" },
            { "VALIDATION_EMAIL", "{0} must be a valid email address" },
            { "VALIDATION_URL", "{0} must be a valid URL" },
        };

        var ptMessages = new Dictionary<string, string>
        {
            { "PROPERTY_REQUIRED", "{0} é obrigatório" },
            { "INVALID_FORMAT", "{0} possui um formato inválido" },
            { "INVALID_LENGTH", "{0} deve ter entre {1} e {2} caracteres" },
            { "INVALID_EMAIL", "{0} deve ser um endereço de email válido" },
            { "INVALID_URL", "{0} deve ser uma URL válida" },
            { "VALUE_NOT_FOUND", "Nenhum registro encontrado com o valor fornecido" },
            { "CONFLICT_ERROR", "Um registro com este valor já existe" },
            { "UNAUTHORIZED", "Você não tem permissão para realizar esta ação" },
            { "FORBIDDEN", "O acesso a este recurso é proibido" },
            { "INTERNAL_ERROR", "Ocorreu um erro interno do servidor" },
            { "VALIDATION_REQUIRED", "{0} é obrigatório" },
            { "VALIDATION_INVALIDFORMAT", "{0} possui um formato inválido" },
            { "VALIDATION_LENGTH", "{0} deve ter entre {1} e {2} caracteres" },
            { "VALIDATION_EMAIL", "{0} deve ser um endereço de email válido" },
            { "VALIDATION_URL", "{0} deve ser uma URL válida" },
        };

        lock (_lockObject)
        {
            _messages["en"] = enMessages;
            _messages["pt-BR"] = ptMessages;
            _messages["pt"] = ptMessages;
        }
    }
}
