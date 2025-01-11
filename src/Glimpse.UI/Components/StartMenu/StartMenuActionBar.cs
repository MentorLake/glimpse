using System.Reactive.Linq;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.Images;
using Glimpse.Libraries.System.Reactive;
using Glimpse.UI.Components.Shared;
using MentorLake.GdkPixbuf;
using MentorLake.Gtk;

namespace Glimpse.UI.Components.StartMenu;

public class StartMenuActionBar
{
	private readonly GtkBoxHandle _root;
	public GtkWidgetHandle Widget => _root;
	public IObservable<string> CommandInvoked { get; private set; }

	public StartMenuActionBar(IObservable<ActionBarViewModel> viewModel)
	{
		_root = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0);
		var userImage = GtkImageHandle.New().AddClass("start-menu__account-icon");

		viewModel
			.Select(vm => vm.UserIconPath)
			.DistinctUntilChanged()
			.TakeUntilDestroyed(_root)
			.Select(path => string.IsNullOrEmpty(path) || !File.Exists(path) ? Assets.Person.ScaleSimple(42, 42, GdkInterpType.GDK_INTERP_BILINEAR) : GdkPixbufFactory.From(path))
			.Select(p => p.ScaleSimple(42, 42, GdkInterpType.GDK_INTERP_BILINEAR))
			.SubscribeDebug(p => userImage.SetFromPixbuf(p));

		var userButton = GtkButtonHandle.New()
			.AddClass("start-menu__user-settings-button").AddMany(
				GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0).AddMany(
					userImage,
					GtkLabelHandle.New(Environment.UserName).AddClass("start-menu__username")));

		userButton.SetValign(GtkAlign.GTK_ALIGN_CENTER);

		var settingsButton = GtkButtonHandle.New()
			.Add(GtkImageHandle.New().SetFromIconName("emblem-system-symbolic", GtkIconSize.GTK_ICON_SIZE_SMALL_TOOLBAR).SetPixelSize(24))
			.AddClass("start-menu__settings")
			.SetValign(GtkAlign.GTK_ALIGN_CENTER)
			.SetHalign(GtkAlign.GTK_ALIGN_END);

		var powerButton = GtkButtonHandle.New()
			.Add(GtkImageHandle.NewFromPixbuf(Assets.Power.ScaleSimple(24, 24, GdkInterpType.GDK_INTERP_BILINEAR)))
			.AddClass("start-menu__power")
			.SetValign(GtkAlign.GTK_ALIGN_CENTER)
			.SetHalign(GtkAlign.GTK_ALIGN_END);

		_root.SetHexpand(true);
		_root.SetVexpand(true);
		_root.AddClass("start-menu__action-bar");
		_root.AddMany(userButton, GtkLabelHandle.New(Environment.MachineName).SetHexpand(true).SetVexpand(true), settingsButton, powerButton);

		CommandInvoked = userButton.ObserveEvent(w => w.Signal_ButtonReleaseEvent().WithLatestFrom(viewModel).Select(t => t.Second.UserSettingsCommand))
			.Merge(powerButton.ObserveEvent(w => w.Signal_ButtonReleaseEvent().WithLatestFrom(viewModel).Select(t => t.Second.PowerButtonCommand)))
			.Merge(settingsButton.ObserveEvent(w => w.Signal_ButtonReleaseEvent().WithLatestFrom(viewModel).Select(t => t.Second.SettingsButtonCommand)));
	}
}
