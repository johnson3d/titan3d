using System.Collections;

namespace Jither.OpenEXR;

public abstract class EXRPartDataHandlerList<T> : IReadOnlyList<T> where T : EXRPartDataHandler
{
    private readonly List<T> parts = new();
    private readonly Dictionary<string, T> partsByName = new();

    public T this[int index] => parts[index];
    public T this[string name] => partsByName[name];

    public int Count => parts.Count;

    internal void Add(string? name, T handler)
    {
        parts.Add(handler);
        if (name != null)
        {
            partsByName.Add(name, handler);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return parts.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
