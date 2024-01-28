using System.Collections.Immutable;
using MentorLake.Redux;
using MentorLake.Redux.Reducers;
using MentorLake.Redux.Selectors;

namespace Glimpse.Common.DesktopEntries;

public class DesktopFileSelectors
{
	public static readonly ISelector<DataTable<string, DesktopFile>> DesktopFiles = SelectorFactory.CreateFeature<DataTable<string, DesktopFile>>();
	public static readonly ISelector<ImmutableList<DesktopFile>> AllDesktopFiles =
		SelectorFactory.Create(DesktopFiles, s => s.ById.Values
			.OrderBy(f => f.Name)
			.ToImmutableList());
}

public class UpdateDesktopFilesAction
{
	public ImmutableList<DesktopFile> DesktopFiles { get; init; }
}

public class DesktopFileReducers
{
	public static readonly FeatureReducerCollection AllReducers =
	[
		FeatureReducer.Build(new DataTable<string, DesktopFile>())
			.On<UpdateDesktopFilesAction>((s, a) => s.UpsertMany(a.DesktopFiles))
	];
}
