namespace OnForkHub.CrossCutting.Localization;

/// <summary>
/// Provides error message localization support.
/// </summary>
public interface IErrorMessageLocalizer
{
    /// <summary>
    /// Gets a localized error message by error code and language.
    /// </summary>
    /// <param name="errorCode">The error code identifier.</param>
    /// <param name="languageCode">The ISO language code (e.g., "en", "pt-BR").</param>
    /// <param name="parameters">Optional parameters to interpolate in the message.</param>
    /// <returns>The localized error message.</returns>
    string GetMessage(string errorCode, string languageCode = "en", params object[]? parameters);

    /// <summary>
    /// Gets a validation error message for a specific property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="errorType">The validation error type (e.g., "Required", "InvalidFormat").</param>
    /// <param name="languageCode">The ISO language code.</param>
    /// <param name="parameters">Optional parameters to interpolate in the message.</param>
    /// <returns>The localized validation error message.</returns>
    string GetValidationMessage(string propertyName, string errorType, string languageCode = "en", params object[]? parameters);

    /// <summary>
    /// Gets all available languages.
    /// </summary>
    /// <returns>A list of supported language codes.</returns>
    IReadOnlyList<string> GetSupportedLanguages();

    /// <summary>
    /// Registers a custom error message.
    /// </summary>
    /// <param name="errorCode">The error code identifier.</param>
    /// <param name="languageCode">The ISO language code.</param>
    /// <param name="message">The error message template.</param>
    void RegisterMessage(string errorCode, string languageCode, string message);
}
