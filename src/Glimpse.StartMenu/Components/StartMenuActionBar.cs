using System.Reactive.Linq;
using Glimpse.Common.Gtk;
using Glimpse.Common.Images;
using Gtk;

namespace Glimpse.StartMenu.Components;

public class StartMenuActionBar : Box
{
	public IObservable<string> CommandInvoked { get; private set; }

	public StartMenuActionBar(IObservable<ActionBarViewModel> viewModel)
		: base(Orientation.Horizontal, 0)
	{
		var userImage = new Image().AddClass("start-menu__account-icon");

		viewModel
			.Select(vm => vm.UserIconPath)
			.DistinctUntilChanged()
			.TakeUntilDestroyed(this)
			.Select(path => string.IsNullOrEmpty(path) || !File.Exists(path) ? Assets.Person.Scale(42) : GlimpseImageFactory.From(path))
			.Select(p => p.Scale(42).Pixbuf)
			.Subscribe(p => userImage.Pixbuf = p);

		var userButton = new Button()
			.AddClass("start-menu__user-settings-button").AddMany(
				new Box(Orientation.Horizontal, 0).AddMany(
					userImage,
					new Label(Environment.UserName).AddClass("start-menu__username")));

		userButton.Valign = Align.Center;

		var settingsButton = new Button(new Image() { IconName = "emblem-system-symbolic", PixelSize = 24 });
		settingsButton.AddClass("start-menu__settings");
		settingsButton.Valign = Align.Center;
		settingsButton.Halign = Align.End;

		var powerButton = new Button(new Image(Assets.Power.Scale(24).Pixbuf));
		powerButton.AddClass("start-menu__power");
		powerButton.Valign = Align.Center;
		powerButton.Halign = Align.End;

		Expand = true;
		this.AddClass("start-menu__action-bar");
		this.AddMany(userButton, new Label(Environment.MachineName) { Expand = true }, settingsButton, powerButton);

		CommandInvoked = userButton.ObserveButtonRelease().WithLatestFrom(viewModel).Select(t => t.Second.UserSettingsCommand)
			.Merge(powerButton.ObserveButtonRelease().WithLatestFrom(viewModel).Select(t => t.Second.PowerButtonCommand))
			.Merge(settingsButton.ObserveButtonRelease().WithLatestFrom(viewModel).Select(t => t.Second.SettingsButtonCommand));
	}
}
