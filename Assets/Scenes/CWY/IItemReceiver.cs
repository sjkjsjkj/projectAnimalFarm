public interface IItemReceiver
{
    bool TryAddItem(string itemId, int amount);
}
