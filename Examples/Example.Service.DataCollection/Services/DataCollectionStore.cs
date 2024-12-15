using Example.Shared.Contracts;
using System.Collections.Concurrent;

namespace Example.Service.DataCollection.Services;

public class DataCollectionStore
{
    private readonly ConcurrentBag<InsertDataRequest> _list = [];
    public void Add(InsertDataRequest data)
    {
        _list.Add(data);
    }
    public InsertDataRequest[] Get()
    {
        return [.. _list];
    }

    public void Clear()
    {
        _list.Clear();
    }
}