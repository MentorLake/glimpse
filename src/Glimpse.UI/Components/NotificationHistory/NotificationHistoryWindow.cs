using System.Reactive.Linq;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System.Reactive;
using Glimpse.Services.Notifications;
using Glimpse.UI.Components.NotificationsConfig;
using Glimpse.UI.Components.Shared;
using Glimpse.UI.Components.Shared.Accordion;
using MentorLake.Gtk;
using MentorLake.Pango;
using MentorLake.Redux;

namespace Glimpse.UI.Components.NotificationHistory;

public class NotificationHistoryWindow
{
	private readonly GtkBoxHandle _root;
	private readonly XorgNotificationsService _xorgNotificationsService;

	public GtkWidgetHandle Widget => _root;

	public NotificationHistoryWindow(ReduxStore store, NotificationsConfigWindow notificationsConfigWindow, XorgNotificationsService xorgNotificationsService)
	{
		_root = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0);
		_xorgNotificationsService = xorgNotificationsService;

		var accordion = new Accordion();
		accordion.Widget.SetHexpand(true);
		accordion.Widget.SetVexpand(true);

		var viewModelObs = store
			.Select(SidePaneSelectors.ViewModel)
			.Select(vm => vm.NotificationHistory.OrderBy(n => n.AppName).ThenBy(n => n.CreationDate))
			.ObserveOn(GLibExt.Scheduler)
			.Replay(1);

		viewModelObs
			.UnbundleMany(e => e.AppName)
			.RemoveIndex()
			.Subscribe(obs =>
			{
				var closeButton = GtkImageHandle.New();

				var sectionHeader = new GtkEventBoxHandle()
					.AddButtonStates()
					.AddClass("button")
					.ObserveEvent(w => w.Signal_EnterNotifyEvent(), _ => closeButton.SetVisible(true))
					.Prop(w => w.ObserveEvent(_ => w.Signal_LeaveNotifyEvent(), _ => closeButton.SetVisible(w.IsPointerInside())))
					.AddMany(GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 4)
						.AddClass("notification-history__section-header")
						.Prop(b => b.SetHalign(GtkAlign.GTK_ALIGN_FILL))
						.AddMany(GtkImageHandle.New().BindViewModel(obs.Select(x => x.AppIcon), 16))
						.AddMany(GtkLabelHandle.New(obs.Key.AppName).SetXalign(0).SetHexpand(true))
						.AddMany(GtkButtonHandle.New()
							.ObserveEvent(w => w.Signal_ButtonReleaseEvent(), e =>
							{
								e.ReturnValue = true;
								_xorgNotificationsService.RemoveHistoryForApplication(obs.Key.AppName);
							}))
						.AddMany(closeButton
							.Prop(i => i.SetFromIconName("window-close-symbolic", GtkIconSize.GTK_ICON_SIZE_SMALL_TOOLBAR))
							.Prop(i => i.SetPixelSize(16))));

				accordion.AddSection(obs.Key.AppName, sectionHeader);
				obs.TakeLast(1).Subscribe(_ => accordion.RemoveSection(obs.Key.AppName));
				closeButton.SetVisible(false);
			});

		viewModelObs
			.UnbundleMany(e => e.Id)
			.RemoveIndex()
			.Subscribe(o =>
			{
				var item = CreateNotificationEntry(o);
				accordion.AddItemToSection(o.Key.AppName, item);
				o.TakeLast(1).Subscribe(_ => accordion.RemoveItemFromSection(o.Key.AppName, item));
			});

		_root.Add(GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_VERTICAL, 8)
			.SetHexpand(true)
			.SetVexpand(true)
			.SetHalign(GtkAlign.GTK_ALIGN_FILL)
			.SetValign(GtkAlign.GTK_ALIGN_FILL)
			.AddClass("notifications-history__container")
			.AddMany(GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0)
				.SetHexpand(false)
				.SetVexpand(false)
				.AddMany(GtkLabelHandle.New("Notifications")
					.AddClass("notifications-history__header")
					.SetHexpand(true)
					.SetVexpand(true)
					.SetXalign(0))
				.AddMany(GtkButtonHandle.New()
					.AddButtonStates()
					.ObserveEvent(w => w.Signal_ButtonReleaseEvent(), _ => notificationsConfigWindow.ShowAndCenterOnScreen())
					.Prop(b => b
						.AddMany(GtkImageHandle.New()
							.SetFromIconName("emblem-system-symbolic", GtkIconSize.GTK_ICON_SIZE_SMALL_TOOLBAR)
							.SetPixelSize(16)))
					.AddClass("notifications-history__clear-all-button"))
				.AddMany(GtkButtonHandle.New()
					.SetLabel("Clear all")
					.AddButtonStates()
					.ObserveEvent(w => w.Signal_ButtonReleaseEvent(), _ => _xorgNotificationsService.ClearHistory())
					.AddClass("notifications-history__clear-all-button")))
			.AddMany(GtkScrolledWindowHandle.New(null, null)
				.SetPolicy(GtkPolicyType.GTK_POLICY_NEVER, GtkPolicyType.GTK_POLICY_AUTOMATIC)
				.SetHexpand(true)
				.SetVexpand(true)
				.AddMany(accordion.Widget)));

		_root.ObserveEvent(e => e.Signal_Map(), _ => accordion.ShowFirstSection());
		viewModelObs.Connect();
		_root.ShowAll();
	}

	public GtkWidgetHandle CreateNotificationEntry(IGroupedObservable<NotificationEntryViewModel, NotificationEntryViewModel> obs)
	{
		var displayedTime = GtkLabelHandle.New("")
			.AddClass("notifications-history-item__displayed-time")
			.SetHalign(GtkAlign.GTK_ALIGN_FILL)
			.SetEllipsize(PangoEllipsizeMode.PANGO_ELLIPSIZE_END)
			.SetMaxWidthChars(1)
			.SetHexpand(true)
			.SetXalign(0);

		var summary = GtkLabelHandle.New("")
			.AddClass("notifications-history-item__summary")
			.SetHalign(GtkAlign.GTK_ALIGN_FILL)
			.SetEllipsize(PangoEllipsizeMode.PANGO_ELLIPSIZE_END)
			.SetMaxWidthChars(1)
			.SetHexpand(true)
			.SetXalign(0)
			.SetYalign(0);

		var body = GtkLabelHandle.New("")
			.AddClass("notifications-history-item__body")
			.SetEllipsize(PangoEllipsizeMode.PANGO_ELLIPSIZE_END)
			.SetLines(5)
			.SetLineWrap(true)
			.SetLineWrapMode(PangoWrapMode.PANGO_WRAP_WORD)
			.SetMaxWidthChars(1)
			.SetXalign(0)
			.SetYalign(0);

		var closeButton = GtkImageHandle.New()
			.SetFromIconName("window-close-symbolic", GtkIconSize.GTK_ICON_SIZE_SMALL_TOOLBAR)
			.SetPixelSize(16);

		var eventBox = GtkEventBoxHandle.New()
			.AddButtonStates()
			.AddClass("button")
			.ObserveEvent(x => x.Signal_EnterNotifyEvent(), _ => closeButton.SetVisible(true))
			.Prop(w => w.ObserveEvent(_ => w.Signal_LeaveNotifyEvent(), _ => closeButton.SetVisible(w.IsPointerInside())))
			.ObserveEvent(w => w.Signal_ButtonReleaseEvent(), _ => _xorgNotificationsService.RemoveHistoryItem(obs.Key.Id))
			.AddMany(GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_VERTICAL, 0)
				.AddClass("notifications-history-item__container")
				.AddMany(GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 4)
					.AddMany(displayedTime)
					.AddMany(GtkButtonHandle.New()
						.AddButtonStates()
						.ObserveEvent(w => w.Signal_ButtonReleaseEvent(), _ => _xorgNotificationsService.RemoveHistoryItem(obs.Key.Id))
						.AddMany(closeButton)
						.SetHalign(GtkAlign.GTK_ALIGN_END)))
				.AddMany(GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0)
					.AddMany(GtkImageHandle.New()
						.AddClass("notifications-history-item__image")
						.BindViewModel(obs.Select(s => s.Image).DistinctUntilChanged(), 34)
						.SetAlignment(0, 0))
					.AddMany(GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_VERTICAL, 0)
						.AddMany(summary)
						.AddMany(body))));

		obs.Subscribe(n =>
		{
			displayedTime.SetText(n.CreationDate.ToString("g"));
			summary.SetText(n.Summary);
			body.SetText(n.Body);
			body.SetVisible(!string.IsNullOrEmpty(n.Body));
		});

		eventBox.ShowAll();
		closeButton.Hide();
		return eventBox;
	}
}
