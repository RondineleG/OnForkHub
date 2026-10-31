namespace OnForkHub.Application.Dtos.Base;

public class PaginationRequest
{
    public int ItemsPerPage { get; set; } = 25;

    public int Page { get; set; } = 1;
}
