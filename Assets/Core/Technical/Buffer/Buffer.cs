/// Buffer in Mesh Cutter Project
/// --- 	SUMMARY : 	---
///
/// --- 	NOTES : 	---
///
using System;

public class Buffer<T>
{
    #region Fields and Properties
    public const int DEFAULT_EXPAND_SIZE = 1;
    private readonly int expandSize = DEFAULT_EXPAND_SIZE;

    /// <summary>
    /// Total count of the buffer.
    /// </summary>
    public int Count = 0;

    /// <summary>
    /// Array of the buffer. Its size should not be set externally.
    /// </summary>
    public T[] Array = new T[] { };
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new buffer (one way expanding array).
    /// </summary>
    /// <param name="_expandSize">Amount by wich the array is expanded
    /// each time it needs more space.</param>
    public Buffer(int _expandSize = DEFAULT_EXPAND_SIZE)
    {
        expandSize = _expandSize;
    }

    /// <summary>
    /// Creates a new buffer (one way expanding array).
    /// </summary>
    /// <param name="_size">Initial size of the buffer.</param>
    /// <param name="_expandSize">Amount by wich the array is expanded
    /// each time it needs more space.</param>
    public Buffer(int _size, int _expandSize) : this(_expandSize)
    {
        Array = new T[_size];
        Count = _size;
    }
    #endregion

    #region Operators
    public T this[int _index]
    {
        get => Array[_index];
        set => Array[_index] = value;
    }
    #endregion

    #region Add and Remove Entries
    /// <summary>
    /// Adds a new element in the array.
    /// </summary>
    /// <param name="_element">New array element.</param>
    public void Add(T _element)
    {
        if (Array.Length == Count)
            Expand();

        Array[Count] = _element;
        Count++;
    }

    /// <summary>
    /// Removes an element from the array.
    /// </summary>
    /// <param name="_element">Element to remove.</param>
    public void Remove(T _element)
    {
        int _index = System.Array.IndexOf(Array, _element);
        RemoveAt(_index);
    }

    /// <summary>
    /// Removes the element at specified index from the array.
    /// </summary>
    /// <param name="_element">Index of the element to remove.</param>
    public void RemoveAt(int _index)
    {
        int _last = Count - 1;
        if (_index != _last)
        {
            T _removed = Array[_index];

            Array[_index] = Array[_last];
            Array[_last] = _removed;
        }

        Count--;
    }

    // -----------------------

    /// <summary>
    /// Clear this collection by setting its count to zero.
    /// </summary>
    public void Clear()
    {
        Count = 0;
    }

    /// <summary>
    /// Completely resets this buffer by removing all entries and setting its count to 0.
    /// </summary>
    public void Reset()
    {
        Array = new T[] { };
        Count = 0;
    }
    #endregion

    #region Resize 
    /// <summary>
    /// Resize this collection content by removing empty entries.
    /// Does not delete existing elements.
    /// </summary>
    public void Resize()
    {
        System.Array.Resize(ref Array, Count);
    }

    // -----------------------

    private void Expand()
    {
        System.Array.Resize(ref Array, Count + expandSize);
    }
    #endregion

    #region Utility
    /// <summary>
    /// Does the Array of the Buffer contains an entry?
    /// </summary>
    /// <param name="_element">Entry to test</param>
    /// <returns>Return true if the element is contained in the array, return false otherwise</returns>
    public bool Contains(T _element)
    {
        for (int i = 0; i < Array.Length; i++)
        {
            if (Array[i].Equals(_element))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Does the Array of the Buffer contains an entry?
    /// </summary>
    /// <param name="_element">Entry to test</param>
    /// <param name="_index">Entry of the value if the value is contained in the array</param>
    /// <returns> Return true if the element is contained in the array, return false otherwise
    /// Return the <see cref="_index"/> index of the value</returns>
    public bool Contains(T _element, out int _index)
    {
        for (int i = 0; i < Array.Length; i++)
        {
            if (Array[i].Equals(_element))
            {
                _index = i;
                return true;
            }
        }
        _index = -1;
        return false;
    }
    #endregion
}
