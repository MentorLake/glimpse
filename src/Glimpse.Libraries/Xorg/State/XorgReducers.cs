using MentorLake.GdkPixbuf;
using MentorLake.Redux;
using MentorLake.Redux.Reducers;

namespace Glimpse.Libraries.Xorg.State;

internal class XorgReducers
{
	public static readonly FeatureReducerCollection Reducers = new()
	{
		FeatureReducer.Build(new DataTable<ulong, WindowProperties>())
			.On<RemoveWindowAction>((s, a) => s.Remove(a.WindowProperties))
			.On<UpdateWindowAction>((s, a) => s.UpsertOne(a.WindowProperties))
			.On<AddWindowAction>((s, a) => s.UpsertOne(a.WindowProperties)),
		FeatureReducer.Build(new DataTable<ulong, GdkPixbufHandle>())
			.On<RemoveWindowAction>((s, a) => s.Remove(a.WindowProperties.WindowRef.Id))
			.On<UpdateScreenshotsAction>((s, a) => s.UpsertMany(a.Screenshots))
	};
}
