namespace OnForkHub.Application.Dtos.Base;

public class PaginationRequestDto
{
    public int Page { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 25;
}
