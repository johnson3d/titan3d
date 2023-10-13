using System.Numerics;

namespace Jither.OpenEXR.Compression;

internal sealed class MinHeap<T> where T: IComparable<T>
{
    private readonly T[] elements;
    private int count;

    public bool IsEmpty => count == 0;

    public MinHeap(int capacity)
    {
        elements = new T[capacity];
    }

    public MinHeap(IEnumerable<T> elements, int count) : this(count)
    {
        foreach (var element in elements)
        {
            Push(element);
        }
    }

    private static bool IsRoot(int index) => index == 0;
    private bool HasChildLeft(int index) => GetChildIndexLeft(index) < count;
    private bool HasChildRight(int index) => GetChildIndexRight(index) < count;

    private static int GetParentIndex(int index) => (index - 1) / 2;
    private static int GetChildIndexLeft(int index) => 2 * index + 1;
    private static int GetChildIndexRight(int index) => 2 * index + 2;

    private T GetParent(int index) => elements[GetParentIndex(index)];
    private T GetChildLeft(int index) => elements[GetChildIndexLeft(index)];
    private T GetChildRight(int index) => elements[GetChildIndexRight(index)];

    private void Swap(int a, int b)
    {
        (elements[a], elements[b]) = (elements[b], elements[a]);
    }

    public void Push(T element)
    {
        if (count == elements.Length)
        {
            throw new IndexOutOfRangeException();
        }

        elements[count] = element;
        count++;

        AdjustUp();
    }

    public T Pop()
    {
        if (count == 0)
        {
            throw new IndexOutOfRangeException();
        }

        var result = elements[0];
        elements[0] = elements[count - 1];
        count--;

        AdjustDown();

        return result;
    }

    public T Peek()
    {
        if (count == 0)
        {
            throw new IndexOutOfRangeException();
        }

        return elements[0];
    }

    private void AdjustDown()
    {
        int index = 0;
        while (HasChildLeft(index))
        {
            var smallerIndex = GetChildIndexLeft(index);
            if (HasChildRight(index) && GetChildRight(index).CompareTo(GetChildLeft(index)) < 0)
            {
                smallerIndex = GetChildIndexRight(index);
            }

            if (elements[smallerIndex].CompareTo(elements[index]) >= 0)
            {
                break;
            }

            Swap(smallerIndex, index);
            index = smallerIndex;
        }
    }

    private void AdjustUp()
    {
        var index = count - 1;
        while (!IsRoot(index) && elements[index].CompareTo(GetParent(index)) < 0)
        {
            var parentIndex = GetParentIndex(index);
            Swap(parentIndex, index);
            index = parentIndex;
        }
    }
}
