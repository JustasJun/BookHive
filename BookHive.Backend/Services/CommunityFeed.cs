using BookHive.Models;

namespace BookHive.Services;

public class CommunityFeed : ICommunityFeed
{
    private const int MaxItems = 15;

    private readonly object _lock = new();
    private readonly List<CommunityReviewItem> _items = new();

    public void Add(CommunityReviewItem item)
    {
        lock (_lock)
        {
            _items.Insert(0, item);

            if (_items.Count > MaxItems)
            {
                _items.RemoveAt(_items.Count - 1);
            }
        }
    }

    public bool Remove(int reviewId)
    {
        lock (_lock)
        {
            return _items.RemoveAll(item => item.ReviewId == reviewId) > 0;
        }
    }

    public List<CommunityReviewItem> GetLatest()
    {
        lock (_lock)
        {
            return new List<CommunityReviewItem>(_items);
        }
    }
}