/*
	Zach Lesperance, CSCD371
	10/03/2013
	
	This class is a data stucture (Set) that holds
	unique records. The set will not accept the
	addition of a duplicate value.
	
	This classes uses the ArrayList data sturucture
	behind the scenes for most of its methods and
	properties
*/
using System;
using System.Collections;
using System.Collections.Generic;

public class Set<T> : ICollection<T>, IEnumerable<T>
{
	private List<T> data;
    private List<T> exclude;
	//DVC
	public Set ()
	{
        data = new List<T>();
	}
    public Set (params IEnumerable<T>[] exclusions)
    {
        data = new List<T>();
        exclude = new List<T>();
        foreach (IEnumerable<T> collection in exclusions)
        {
            foreach (T item in collection)
            {
                exclude.Add(item);
            }
        }
    }
	
	//Properties	
	public bool Empty {
		get {
			return (data.Count == 0);
		}
	}
	public int Count {
		get {
			return data.Count;
		}
	}
    public bool IsReadOnly
    {
        get
        {
            return false;
        }
    }
	//ICollection Properties
	/*public bool IsSynchronized {
		get {
			return data.IsSynchronized;
		}
	}	
	public object SyncRoot {
		get {
			return data.SyncRoot;
		}
	}*/
	
	//Iterator
	public T this[int index] {
		get
        {
			return data[index];
		}
        set
        {
            data[index] = value;
        }
	}
	
	//Methods
	public bool Contains (T o) {
		if (o == null)
			return false;
		return (data.Contains(o) || (exclude != null && exclude.Contains(o)));
	}
    public void Add(T o)
    {
		if (o == null || this.Contains(o))
			return;
		data.Add(o);
	}
    public bool Remove(T o)
    {
		if (o == null || !this.Contains(o))
			return false;
		data.Remove(o);
		return true;
	}
    public void Clear()
    {
        while (data.Count > 0)
            data.Remove(data[0]);
    }
    public override bool Equals(object o)
    {
		if (o == null || !(o is Set<T>))
			return false;
		Set<T> that = (Set<T>) o;
		if (this.Count != that.Count)
			return false;
		foreach (T item in data)
		{
			if(!that.Contains(item))
				return false;
		}
		return true;
	}	
	public override int GetHashCode()
	{
		int code = 0;
		foreach (object item in data)
			code += item.GetHashCode();
		return code;
	}	
	public override string ToString()
	{
		string s = "[ ";
		foreach (object item in data)
			s += item.ToString() + ", ";
		s += " ]";
		return s;
	}
    public List<T> ToList()
    {
        List<T> l = new List<T>();
        foreach (T item in data)
        {
            l.Add(item);
        }
        return l;
    }
    public void CopyTo(T[] array, int index)
	{
		data.CopyTo(array, index);
	}
	public System.Collections.Generic.IEnumerator<T> GetEnumerator()
	{
		return data.GetEnumerator();
	}

    IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}