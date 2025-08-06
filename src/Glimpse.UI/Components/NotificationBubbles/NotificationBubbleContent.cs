using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Libraries.Gtk;
using Glimpse.UI.Components.Shared;
using MentorLake.Gtk;
using MentorLake.Pango;
using Unit = System.Reactive.Unit;

namespace Glimpse.UI.Components.NotificationBubbles;

public class NotificationBubbleContent
{
	private readonly GtkBoxHandle _root;

	public GtkWidgetHandle Widget => _root;

	private readonly Subject<string> _actionInvokedSubject = new();
	public IObservable<string> ActionInvoked => _actionInvokedSubject;

	private readonly Subject<Unit> _closeNotificationSubject = new();
	public IObservable<Unit> CloseNotification => _closeNotificationSubject;

	public NotificationBubbleContent(IObservable<NotificationBubbleViewModel> notificationStateObs)
	{
		_root = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0);

		var appName = GtkLabelHandle.New("")
			.Prop(w => w.AddClass("notifications__app-name"))
			.Prop(w => w.SetHalign(GtkAlign.GTK_ALIGN_FILL))
			.Prop(w => w.SetEllipsize(PangoEllipsizeMode.PANGO_ELLIPSIZE_END))
			.Prop(w => w.SetMaxWidthChars(1))
			.Prop(w => w.SetHexpand(true))
			.Prop(w => w.SetXalign(0));

		var summary = GtkLabelHandle.New("")
			.Prop(w => w.AddClass("notifications__summary"))
			.Prop(w => w.SetHalign(GtkAlign.GTK_ALIGN_FILL))
			.Prop(w => w.SetEllipsize(PangoEllipsizeMode.PANGO_ELLIPSIZE_END))
			.Prop(w => w.SetMaxWidthChars(1))
			.Prop(w => w.SetHexpand(true))
			.Prop(w => w.SetXalign(0))
			.Prop(w => w.SetYalign(0));

		var body = GtkLabelHandle.New("")
			.Prop(w => w.AddClass("notifications__body"))
			.Prop(w => w.SetEllipsize(PangoEllipsizeMode.PANGO_ELLIPSIZE_END))
			.Prop(w => w.SetLines(5))
			.Prop(w => w.SetLineWrap(true))
			.Prop(w => w.SetLineWrapMode(PangoWrapMode.PANGO_WRAP_WORD))
			.Prop(w => w.SetMaxWidthChars(1))
			.Prop(w => w.SetXalign(0))
			.Prop(w => w.SetYalign(0));

		var closeButton = GtkButtonHandle.New()
			.AddButtonStates()
			.AddMany(GtkImageHandle.New().SetFromIconName("window-close-symbolic", GtkIconSize.GTK_ICON_SIZE_SMALL_TOOLBAR).SetPixelSize(16))
			.SetHalign(GtkAlign.GTK_ALIGN_END)
			.ObserveEvent(w => w.Signal_ButtonReleaseEvent(), _ => _closeNotificationSubject.OnNext(Unit.Default));

		var appIcon = GtkImageHandle.New()
			.BindViewModel(notificationStateObs.Select(s => s.AppIcon).DistinctUntilChanged(), 16);

		var image = GtkImageHandle.New()
			.AddClass("notifications__image")
			.BindViewModel(notificationStateObs.Select(s => s.Image).DistinctUntilChanged(), 34)
			.SetAlignment(0, 0);

		var appNameRow = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 4)
			.AddMany(appIcon, appName, closeButton);

		var textColumn = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_VERTICAL, 0)
			.AddMany(summary, body);

		var mainRow = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0)
			.AddMany(image, textColumn);

		var actionsRow = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 8)
			.Prop(w => w.SetHalign(GtkAlign.GTK_ALIGN_END));

		var layout = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_VERTICAL, 0)
			.AddClass("notifications__window")
			.AddMany(appNameRow, mainRow, actionsRow);

		_root.Add(layout);

		notificationStateObs.Subscribe(n =>
		{
			appName.SetText(n.AppName);
			summary.SetText(n.Summary);
			body.SetText(n.Body);
			body.SetVisible(!string.IsNullOrEmpty(n.Body));
			actionsRow.RemoveAllChildren();

			foreach (var action in n.Actions)
			{
				actionsRow.AddMany(GtkButtonHandle.New()
					.Prop(w => w.SetHalign(GtkAlign.GTK_ALIGN_END))
					.Prop(w => w.SetLabel(action))
					.Prop(w => w.AddButtonStates())
					.Prop(w => w.AddClass("notifications_action-button"))
					.ObserveEvent(w => w.Signal_ButtonReleaseEvent(), _ => _actionInvokedSubject.OnNext(action))
					.ShowAll());
			}
		});

		_root.ShowAll();
	}
}
