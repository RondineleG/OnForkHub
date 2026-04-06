namespace OnForkHub.Application.Dtos.Category.Request;

public class CategoryUpdateRequestDto
{
    public CategoryRequestDto Category { get; set; } = new();

    public long Id { get; set; }
}
