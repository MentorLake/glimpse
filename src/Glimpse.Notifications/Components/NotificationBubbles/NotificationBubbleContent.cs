using System.Reactive.Linq;
using System.Reactive.Subjects;
using Gtk;
using Pango;
using Unit = System.Reactive.Unit;
using WrapMode = Pango.WrapMode;

namespace Glimpse.UI.Components.NotificationBubbles;

public class NotificationBubbleContent : Bin
{
	private readonly Subject<string> _actionInvokedSubject = new();
	public IObservable<string> ActionInvoked => _actionInvokedSubject;

	private readonly Subject<Unit> _closeNotificationSubject = new();
	public IObservable<Unit> CloseNotification => _closeNotificationSubject;

	public NotificationBubbleContent(IObservable<NotificationBubbleViewModel> notificationStateObs)
	{
		var appName = new Label();
		appName.AddClass("notifications__app-name");
		appName.Halign = Align.Fill;
		appName.Ellipsize = EllipsizeMode.End;
		appName.MaxWidthChars = 1;
		appName.Hexpand = true;
		appName.Xalign = 0;

		var summary = new Label();
		summary.AddClass("notifications__summary");
		summary.Halign = Align.Fill;
		summary.Ellipsize = EllipsizeMode.End;
		summary.MaxWidthChars = 1;
		summary.Hexpand = true;
		summary.Xalign = 0;
		summary.Yalign = 0;

		var body = new Label();
		body.AddClass("notifications__body");
		body.Ellipsize = EllipsizeMode.End;
		body.Lines = 5;
		body.LineWrap = true;
		body.LineWrapMode = WrapMode.Word;
		body.MaxWidthChars = 1;
		body.Xalign = 0;
		body.Yalign = 0;

		var closeButton = new Button();
		closeButton.AddButtonStates();
		closeButton.Image = new Image() { IconName = "window-close-symbolic", PixelSize = 16 };
		closeButton.Halign = Align.End;
		closeButton.ObserveButtonRelease().Subscribe(_ => _closeNotificationSubject.OnNext(Unit.Default));

		var appIcon = new Image();
		appIcon.BindViewModel(notificationStateObs.Select(s => s.AppIcon).DistinctUntilChanged(), 16);

		var image = new Image().AddClass("notifications__image");
		image.BindViewModel(notificationStateObs.Select(s => s.Image).DistinctUntilChanged(), 34);
		image.Xalign = 0;
		image.Yalign = 0;

		var appNameRow = new Box(Orientation.Horizontal, 4);
		appNameRow.AddMany(appIcon, appName, closeButton);

		var textColumn = new Box(Orientation.Vertical, 0);
		textColumn.AddMany(summary, body);

		var mainRow = new Box(Orientation.Horizontal, 0);
		mainRow.AddMany(image, textColumn);

		var actionsRow = new Box(Orientation.Horizontal, 8);
		actionsRow.Halign = Align.End;

		var layout = new Box(Orientation.Vertical, 0)
			.AddClass("notifications__window")
			.AddMany(appNameRow, mainRow, actionsRow);

		Add(layout);

		notificationStateObs.Subscribe(n =>
		{
			appName.Text = n.AppName;
			summary.Text = n.Summary;
			body.Text = n.Body;
			body.Visible = !string.IsNullOrEmpty(n.Body);

			actionsRow.RemoveAllChildren();

			foreach (var action in n.Actions)
			{
				var actionButton = new Button() { Halign = Align.End, Label = action };
				actionButton.AddButtonStates();
				actionButton.AddClass("notifications_action-button");
				actionButton.ShowAll();
				actionButton.ObserveButtonRelease().Subscribe(_ => _actionInvokedSubject.OnNext(action));
				actionsRow.Add(actionButton);
			}
		});

		ShowAll();
	}
}
