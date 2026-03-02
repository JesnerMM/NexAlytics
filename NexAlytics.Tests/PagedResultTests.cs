using NexAlytics.Application.DTOs;

namespace NexAlytics.Tests;

public class PagedResultTests
{
    [Theory]
    [InlineData(100, 10, 10)]
    [InlineData(101, 10, 11)]
    [InlineData(10,  10,  1)]
    [InlineData(0,   10,  0)]
    [InlineData(1,   10,  1)]
    public void TotalPages_Calculates_Correctly(int total, int pageSize, int expected)
    {
        var result = new PagedResult<string> { TotalCount = total, PageSize = pageSize };

        Assert.Equal(expected, result.TotalPages);
    }

    [Fact]
    public void TotalPages_ZeroPageSize_ReturnsOne()
    {
        var result = new PagedResult<string> { TotalCount = 50, PageSize = 0 };

        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public void HasPrevious_FirstPage_ReturnsFalse()
    {
        var result = new PagedResult<string> { Page = 1, PageSize = 10, TotalCount = 50 };

        Assert.False(result.HasPrevious);
    }

    [Fact]
    public void HasPrevious_SecondPage_ReturnsTrue()
    {
        var result = new PagedResult<string> { Page = 2, PageSize = 10, TotalCount = 50 };

        Assert.True(result.HasPrevious);
    }

    [Fact]
    public void HasNext_LastPage_ReturnsFalse()
    {
        var result = new PagedResult<string> { Page = 5, PageSize = 10, TotalCount = 50 };

        Assert.False(result.HasNext);
    }

    [Fact]
    public void HasNext_NotLastPage_ReturnsTrue()
    {
        var result = new PagedResult<string> { Page = 4, PageSize = 10, TotalCount = 50 };

        Assert.True(result.HasNext);
    }
}
