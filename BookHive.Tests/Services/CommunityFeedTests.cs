using BookHive.Models;
using BookHive.Services;

namespace BookHive.Tests.Services;

public class CommunityFeedTests
{
    private readonly CommunityFeed _feed = new();

    [Fact]
    public void Add_WhenMoreThanFifteenItems_KeepsOnlyTheNewestFifteen()
    {
        for (int id = 1; id <= 20; id++)
        {
            _feed.Add(CreateItem(id));
        }

        List<CommunityReviewItem> latest = _feed.GetLatest();

        Assert.Equal(15, latest.Count);
        Assert.Equal(20, latest[0].ReviewId);
        Assert.Equal(6, latest[^1].ReviewId);
    }

    [Fact]
    public void Remove_WhenItemExists_RemovesItAndReturnsTrue()
    {
        _feed.Add(CreateItem(1));
        _feed.Add(CreateItem(2));

        bool removed = _feed.Remove(1);

        Assert.True(removed);
        Assert.Equal(2, Assert.Single(_feed.GetLatest()).ReviewId);
    }

    [Fact]
    public void Remove_WhenItemDoesNotExist_ReturnsFalse()
    {
        _feed.Add(CreateItem(1));

        Assert.False(_feed.Remove(99));
        Assert.Single(_feed.GetLatest());
    }

    [Fact]
    public void GetLatest_ReturnsACopy_SoChangingItDoesNotAffectTheFeed()
    {
        _feed.Add(CreateItem(1));

        List<CommunityReviewItem> copy = _feed.GetLatest();
        copy.Clear();

        Assert.Single(_feed.GetLatest());
    }

    [Fact]
    public void Add_FromManyThreads_KeepsExactlyFifteenDistinctItems()
    {
        Parallel.For(0, 1000, id => _feed.Add(CreateItem(id)));

        List<CommunityReviewItem> latest = _feed.GetLatest();

        Assert.Equal(15, latest.Count);
        Assert.Equal(15, latest.Select(item => item.ReviewId).Distinct().Count());
    }

    [Fact]
    public void AddReadAndRemove_FromManyThreads_DoesNotThrowAndEndsEmpty()
    {
        Parallel.For(0, 1000, id =>
        {
            _feed.Add(CreateItem(id));
            _feed.GetLatest();
            _feed.Remove(id);
        });

        Assert.Empty(_feed.GetLatest());
    }

    private static CommunityReviewItem CreateItem(int id) =>
        new(id, 1, "user", "Title", "Author", null, 5, "Text", DateTime.UtcNow);
}