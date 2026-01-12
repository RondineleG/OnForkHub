namespace OnForkHub.CrossCutting.Tests.Localization;

[TestClass]
public class ErrorMessageLocalizerTests
{
    private readonly ErrorMessageLocalizer _localizer = new();

    [TestMethod]
    public void GetMessageWithValidEnglishCodeReturnsEnglishMessage()
    {
        var errorCode = "PROPERTY_REQUIRED";

        var message = _localizer.GetMessage(errorCode, "en");

        message.Should().NotBeNullOrEmpty();
        message.Should().Contain("required");
    }

    [TestMethod]
    public void GetMessageWithPortugueseCodeReturnsPortugueseMessage()
    {
        var errorCode = "PROPERTY_REQUIRED";

        var message = _localizer.GetMessage(errorCode, "pt-BR");

        message.Should().NotBeNullOrEmpty();
        message.Should().Contain("obrigatório");
    }

    [TestMethod]
    public void GetMessageWithParametersInterpolatesCorrectly()
    {
        var errorCode = "INVALID_LENGTH";

        var message = _localizer.GetMessage(errorCode, "en", "Name", 3, 50);

        message.Should().Contain("Name");
        message.Should().Contain("3");
        message.Should().Contain("50");
    }

    [TestMethod]
    public void GetMessageWithInvalidCodeReturnsDefaultMessage()
    {
        var errorCode = "INVALID_CODE_XYZ";

        var message = _localizer.GetMessage(errorCode, "en");

        message.Should().Contain(errorCode);
    }

    [TestMethod]
    public void GetMessageWithUnsupportedLanguageFallsBackToEnglish()
    {
        var errorCode = "PROPERTY_REQUIRED";

        var message = _localizer.GetMessage(errorCode, "es");

        message.Should().Contain("required");
    }

    [TestMethod]
    public void GetValidationMessageWithValidPropertyReturnsFormattedMessage()
    {
        var propertyName = "Email";
        var errorType = "Required";

        var message = _localizer.GetValidationMessage(propertyName, errorType, "en");

        message.Should().Contain(propertyName);
        message.Should().Contain("required");
    }

    [TestMethod]
    public void GetValidationMessagePortugueseReturnsPortugueseMessage()
    {
        var propertyName = "Senha";
        var errorType = "Required";

        var message = _localizer.GetValidationMessage(propertyName, errorType, "pt-BR");

        message.Should().Contain("Senha");
        message.Should().Contain("obrigatório");
    }

    [TestMethod]
    public void GetSupportedLanguagesReturnsAvailableLanguages()
    {
        var languages = _localizer.GetSupportedLanguages();

        languages.Should().NotBeEmpty();
        languages.Should().Contain("en");
        languages.Should().Contain("pt-BR");
    }

    [TestMethod]
    public void RegisterMessageWithCustomMessageRegistersSuccessfully()
    {
        var errorCode = "CUSTOM_ERROR";
        var languageCode = "en";
        var message = "This is a custom error message";

        _localizer.RegisterMessage(errorCode, languageCode, message);
        var retrievedMessage = _localizer.GetMessage(errorCode, languageCode);

        retrievedMessage.Should().Be(message);
    }

    [TestMethod]
    public void RegisterMessageWithNullErrorCodeThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _localizer.RegisterMessage(null!, "en", "message"));
    }

    [TestMethod]
    public void GetMessageWithNullErrorCodeThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _localizer.GetMessage(null!));
    }

    [TestMethod]
    public void GetValidationMessageWithNullPropertyNameThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _localizer.GetValidationMessage(null!, "Required"));
    }

    [TestMethod]
    public void GetValidationMessageWithNullErrorTypeThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _localizer.GetValidationMessage("Name", null!));
    }

    [TestMethod]
    public void GetMessageWithPortugueseVariationNormalizesCorrectly()
    {
        var errorCode = "PROPERTY_REQUIRED";

        var messagePt = _localizer.GetMessage(errorCode, "pt");
        var messagePtBr = _localizer.GetMessage(errorCode, "pt-BR");

        messagePt.Should().Be(messagePtBr);
    }
}
