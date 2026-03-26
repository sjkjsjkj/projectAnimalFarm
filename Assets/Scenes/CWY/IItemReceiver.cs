public interface IItemReceiver
{
    bool TryAddItem(EItem itemId, int amount);
}
