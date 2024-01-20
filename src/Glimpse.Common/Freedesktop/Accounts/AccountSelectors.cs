using MentorLake.Redux.Selectors;
using static MentorLake.Redux.Selectors.SelectorFactory;

namespace Glimpse.Freedesktop;

public static class AccountSelectors
{
	private static readonly ISelector<AccountState> s_accountState = CreateFeature<AccountState>();
	public static readonly ISelector<string> UserIconPath = Create(s_accountState, s => s.IconPath);
}
