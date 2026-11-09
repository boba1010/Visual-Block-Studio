using System.Collections;
using XamlBlocksCompiler.Syntax.Controls;

namespace XamlBlocksCompiler.Collections;

public sealed class XamlElementCollection(XamlPanelSyntax parent) : IList<XamlControlSyntax>
{
    private XamlControlSyntax[] _controls = new XamlControlSyntax[8];
    private int _count;

    public XamlControlSyntax this[int index]
    { 
        get => (uint)index < (uint)_count ? _controls[index] : throw new ArgumentOutOfRangeException(nameof(index));
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (ReferenceEquals(_controls[index], value))
                return;

            if (value.Parent is not null)
                throw new InvalidOperationException("Element cannot have more than one parent.");

            _controls[index].Parent = null;
            value.Parent = parent;
            _controls[index] = value;
        }
    }

    public int Count => _count;

    public bool IsReadOnly => false;

    public void Add(XamlControlSyntax syntax)
    {
        if (syntax.Parent is not null)
            throw new InvalidOperationException("Element cannot have more than one parent.");

        EnsureCapacity();

        syntax.Parent = parent;
        _controls[_count++] = syntax;
    }

    private void EnsureCapacity()
    {
        if (_count < _controls.Length)
            return;

        Array.Resize(ref _controls, _controls.Length * 2);
    }

    public void Clear()
    {
        for (int i = 0; i < _count; i++)
            _controls[i]!.Parent = null;

        _count = 0;
    }

    public bool Contains(XamlControlSyntax item)
    {
        return IndexOf(item) >= 0;
    }

    public void CopyTo(XamlControlSyntax[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        Array.Copy(_controls, 0, array, arrayIndex, Count);
    }

    public IEnumerator<XamlControlSyntax> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
            yield return _controls[i];
    }

    public int IndexOf(XamlControlSyntax item)
    {
        for (int i = 0; i < Count; i++)
        {
            if (ReferenceEquals(_controls[i], item))
                return i;
        }

        return -1;
    }

    public void Insert(int index, XamlControlSyntax item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if ((uint)index > (uint)Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (item.Parent is not null)
            throw new InvalidOperationException("Element cannot have more than one parent.");

        EnsureCapacity();

        Array.Copy(_controls, index, _controls, index + 1, Count - index - 1);

        _controls[index] = item;
        item.Parent = parent;
        _count++;
    }

    public bool Remove(XamlControlSyntax syntax)
    {
        int index = IndexOf(syntax);

        if (index < 0)
            return false;

        RemoveAt(index);
        return true;
    }

    public void RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _count);

        var item = _controls[index];

        Array.Copy(_controls, index + 1, _controls, index, _count - index - 1);

        _controls[--_count] = null!;
        item.Parent = null;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
