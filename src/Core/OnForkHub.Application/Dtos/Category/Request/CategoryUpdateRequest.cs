namespace OnForkHub.Application.Dtos.Category.Request;

public class CategoryUpdateRequest
{
    public CategoryRequest Category { get; set; } = new();

    public long Id { get; set; }
}
