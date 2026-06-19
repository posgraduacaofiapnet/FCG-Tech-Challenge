namespace FCG.Domain.Common;

public sealed record PaginationParameters(int Page, int PageSize)
{
    public int Skip => (Page - 1) * PageSize;
}
