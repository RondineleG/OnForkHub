// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Application.Dtos.Category.Request;

public class CategoryUpdateRequestDto
{
    public CategoryRequestDto Category { get; set; } = new();

    public long Id { get; set; }
}
