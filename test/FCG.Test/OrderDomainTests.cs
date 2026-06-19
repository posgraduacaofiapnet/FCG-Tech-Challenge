using FCG.Domain.Common;
using FCG.Domain.Entities;
using FCG.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace FCG.Test;

public class OrderDomainTests
{
    [Fact]
    public void AddItem_ShouldIncreaseTotalAmount_WhenOrderIsPending()
    {
        var order = new Order(Guid.NewGuid());
        var gameId = Guid.NewGuid();

        var result = order.AddItem(gameId, 99.90m);

        result.IsSuccess.Should().BeTrue();
        order.TotalAmount.Should().Be(99.90m);
        order.Items.Should().ContainSingle(orderItem => orderItem.GameId == gameId);
    }

    [Fact]
    public void MarkAsPaid_ShouldChangeStatus_WhenOrderIsPending()
    {
        var order = new Order(Guid.NewGuid());

        var result = order.MarkAsPaid();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void MarkAsPaid_ShouldFail_WhenOrderIsNotPending()
    {
        var order = new Order(Guid.NewGuid());
        order.MarkAsPaid();

        var result = order.MarkAsPaid();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Orders.NotPending(order.Id));
    }

    [Fact]
    public void AddItem_ShouldFail_WhenOrderIsNotPending()
    {
        var order = new Order(Guid.NewGuid());
        order.MarkAsPaid();

        var result = order.AddItem(Guid.NewGuid(), 10);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Orders.CannotAddItemsToNonPendingOrder);
    }

    [Fact]
    public void Cancel_ShouldChangeStatus_WhenOrderIsPending()
    {
        var order = new Order(Guid.NewGuid());

        var result = order.Cancel();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Canceled);
    }

    [Fact]
    public void Cancel_ShouldFail_WhenOrderIsNotPending()
    {
        var order = new Order(Guid.NewGuid());
        order.MarkAsPaid();

        var result = order.Cancel();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Orders.OnlyPendingOrdersCanBeCanceled);
    }
}
