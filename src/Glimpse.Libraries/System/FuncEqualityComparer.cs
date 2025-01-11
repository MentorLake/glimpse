namespace Glimpse.Libraries.System;

public class FuncEqualityComparer<T> : IEqualityComparer<T>
{
	private readonly Func<T, T, bool> _comparison;

	public FuncEqualityComparer(Func<T, T, bool> comparison)
	{
		_comparison = comparison;
	}

	public bool Equals(T x, T y)
	{
		if (ReferenceEquals(x, y))
		{
			return true;
		}

		if (x == null || y == null)
		{
			return false;
		}

		return _comparison(x, y);
	}

	public int GetHashCode(T obj) => obj.GetHashCode();

	public static IEqualityComparer<T> Create(Func<T, T, bool> f)
	{
		return new FuncEqualityComparer<T>(f);
	}
}
