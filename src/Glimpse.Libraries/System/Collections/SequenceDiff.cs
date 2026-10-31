namespace Glimpse.Libraries.System.Collections;

public abstract record SequenceDiff
{
	public record Matched<T>(T Old, T New) : SequenceDiff;
	public record Removed<T>(T Value) : SequenceDiff;
	public record Added<T>(T Value) : SequenceDiff;

	public static List<SequenceDiff> Compute<T>(IReadOnlyList<T> before, IReadOnlyList<T> after)
	{
		int m = before.Count, n = after.Count;
		int[,] dp = new int[m + 1, n + 1];
		for (int i = 0; i <= m; i++) dp[i, 0] = i;
		for (int j = 0; j <= n; j++) dp[0, j] = j;
		for (int i = 1; i <= m; i++)
		{
			for (int j = 1; j <= n; j++)
			{
				int cost = EqualityComparer<T>.Default.Equals(before[i - 1], after[j - 1]) ? 0 : 1;
				dp[i, j] = Math.Min(Math.Min(dp[i - 1, j - 1] + cost, dp[i - 1, j] + 1), dp[i, j - 1] + 1);
			}
		}

		List<SequenceDiff> pairs = new List<SequenceDiff>();
		int x = m, y = n;
		while (x > 0 || y > 0)
		{
			if (x > 0 && y > 0 && dp[x, y] == dp[x - 1, y - 1] + (EqualityComparer<T>.Default.Equals(before[x - 1], after[y - 1]) ? 0 : 1))
			{
				pairs.Add(new Matched<T>(before[x - 1], after[y - 1]));
				x--; y--;
			}
			else if (x > 0 && dp[x, y] == dp[x - 1, y] + 1)
			{
				pairs.Add(new Removed<T>(before[x - 1]));
				x--;
			}
			else
			{
				pairs.Add(new Added<T>(after[y - 1]));
				y--;
			}
		}
		pairs.Reverse();
		return pairs;
	}
}
