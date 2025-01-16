namespace OnForkHub.Core.Enums;

/// <summary>Conditional statements.</summary>
public enum EResultStatus
{
    /// <summary>Success</summary>
    Success,

    /// <summary>Has Validation</summary>
    HasValidation,

    /// <summary>Success</summary>
    HasError,

    /// <summary>Has Error</summary>
    EntityNotFound,

    /// <summary>Entity NotFound</summary>
    EntityHasError,

    /// <summary>Entity Has Error</summary>
    EntityAlreadyExists,

    /// <summary>Entity Already Exists</summary>
    NoContent,
}