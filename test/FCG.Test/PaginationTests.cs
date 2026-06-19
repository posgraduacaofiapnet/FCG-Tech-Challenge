using FCG.Domain.Common;
using FluentAssertions;
using Xunit;

namespace FCG.Test;

public class PaginationTests
{
    [Fact]
    public void PaginationParameters_Skip_ShouldCalculateItemsToSkip()
    {
        var pagination = new PaginationParameters(3, 30);

        pagination.Skip.Should().Be(60);
    }

    [Fact]
    public void PagedResult_TotalPages_ShouldReturnZero_WhenThereAreNoItems()
    {
        var result = new PagedResult<string>([], 1, 30, 0);

        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public void PagedResult_TotalPages_ShouldRoundUp()
    {
        var result = new PagedResult<string>([], 1, 30, 31);

        result.TotalPages.Should().Be(2);
    }
}
