namespace OnForkHub.Application.Dtos.Category.Request;

public class CategoryUpdateRequest
{
    public CategoryRequest Category { get; set; } = new();

    public string Id { get; set; } = string.Empty;
}
