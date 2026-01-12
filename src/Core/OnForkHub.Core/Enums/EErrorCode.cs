namespace OnForkHub.Core.Enums;

/// <summary>
/// Standardized error codes for API responses and validation failures.
/// Follows HTTP status code conventions and custom application codes.
/// </summary>
public enum EErrorCode
{
    // Validation errors (4001-4100)
    ValidationFailed = 4001,
    PropertyRequired = 4002,
    InvalidFormat = 4003,
    InvalidLength = 4004,
    DuplicateEntry = 4005,
    InvalidRange = 4006,
    PatternMismatch = 4007,
    InvalidEmail = 4008,
    InvalidUrl = 4009,
    InvalidPhoneNumber = 4010,

    // Authentication & Authorization (4011-4020)
    Unauthorized = 4011,
    Forbidden = 4012,
    InvalidCredentials = 4013,
    TokenExpired = 4014,
    TokenInvalid = 4015,

    // Not Found (4041-4050)
    NotFound = 4041,
    EntityNotFound = 4042,
    ResourceNotFound = 4043,

    // Conflict (4091-4100)
    Conflict = 4091,
    AlreadyExists = 4092,
    VersionMismatch = 4093,

    // Business Logic errors (5001-5100)
    BusinessRuleViolation = 5001,
    InvalidOperation = 5002,
    OperationFailed = 5003,
    ExternalServiceError = 5004,
    InsufficientPermissions = 5005,
    MaximumAttemptsExceeded = 5006,

    // Internal errors (5101-5200)
    InternalServerError = 5101,
    DatabaseError = 5102,
    ServiceUnavailable = 5103,
    ConfigurationError = 5104,
    UnexpectedError = 5199,
}
