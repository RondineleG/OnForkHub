// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Core.Enums;

public enum EResultStatus
{
    Success,

    HasValidation,

    HasError,

    EntityNotFound,

    EntityHasError,

    EntityAlreadyExists,

    NoContent,
}
