namespace OnForkHub.Application.Dtos.Category.Request;

public class CategoryUpdateRequestDto
{
    public long Id { get; set; }

    public CategoryRequestDto Category { get; set; } = new();
}
