using System.Reactive.Linq;
using System.Reactive.Subjects;
using Glimpse.Libraries.Gtk;
using Glimpse.Libraries.System.Reactive;
using Glimpse.UI.Components.Shared;
using MentorLake.Gtk;
using Microsoft.Extensions.DependencyInjection;
using DateTime = System.DateTime;

namespace Glimpse.UI.Components.Calendar;

public class CalendarWindow
{
	private readonly GtkBoxHandle _root;

	public GtkWidgetHandle Widget => _root;

	public CalendarWindow([FromKeyedServices(Timers.OneSecond)] IObservable<DateTime> oneSecondTimer)
	{
		_root = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0);

		var displayedDateTimeObs = new BehaviorSubject<DateTime>(DateTime.Now);

		var todayLabel = GtkLabelHandle.New("")
			.SetHalign(GtkAlign.GTK_ALIGN_FILL)
			.SetXalign(0)
			.SetSizeRequest(10, 50)
			.AddClass("calendar__today");

		oneSecondTimer
			.DistinctUntilChanged(x => x.Date)
			.ObserveOn(GLibExt.Scheduler)
			.Subscribe(dt => todayLabel.SetText(dt.ToString("dddd, MMMM dd")));

		var monthLabel = GtkLabelHandle.New("")
			.SetHexpand(true)
			.SetVexpand(true)
			.SetXalign(0);

		var monthUpButton = GtkButtonHandle.New()
			.AddButtonStates()
			.Add(GtkImageHandle.New().SetFromIconName("go-up-symbolic", GtkIconSize.GTK_ICON_SIZE_SMALL_TOOLBAR).SetPixelSize(16))
			.ObserveEvent(w => w.Signal_ButtonReleaseEvent().WithLatestFrom(displayedDateTimeObs), t => displayedDateTimeObs.OnNext(t.Second.AddMonths(-1)));

		var monthDownButton = GtkButtonHandle.New()
			.AddButtonStates()
			.Add(GtkImageHandle.New().SetFromIconName("go-down-symbolic", GtkIconSize.GTK_ICON_SIZE_SMALL_TOOLBAR).SetPixelSize(16))
			.ObserveEvent(w => w.Signal_ButtonReleaseEvent().WithLatestFrom(displayedDateTimeObs), t => displayedDateTimeObs.OnNext(t.Second.AddMonths(1)));

		var monthSelectorLayout = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_HORIZONTAL, 0)
			.Add(monthLabel)
			.Add(monthUpButton)
			.Add(monthDownButton)
			.AddClass("calendar__month");

		var layout = GtkBoxHandle.New(GtkOrientation.GTK_ORIENTATION_VERTICAL, 0)
			.SetHexpand(true)
			.SetVexpand(true)
			.SetHalign(GtkAlign.GTK_ALIGN_FILL)
			.SetValign(GtkAlign.GTK_ALIGN_FILL)
			.AddClass("calendar__layout")
			.Add(todayLabel)
			.Add(monthSelectorLayout);

		_root.Add(layout);

		GtkGridHandle currentDateTimeGrid = null;

		displayedDateTimeObs.Subscribe(dt =>
		{
			monthLabel.SetText(dt.ToString("MMMM yyyy"));
			if (currentDateTimeGrid != null) currentDateTimeGrid.Destroy();
			currentDateTimeGrid = CreateDateGrid(dt);
			layout.Add(currentDateTimeGrid);
		});

		_root.ObserveEvent(w => w.Signal_Map(), _ => displayedDateTimeObs.OnNext(DateTime.Now));
	}

	private GtkGridHandle CreateDateGrid(DateTime currentDateTime)
	{
		var dateGrid = GtkGridHandle.New()
			.SetColumnHomogeneous(true)
			.SetRowHomogeneous(true)
			.SetRowSpacing(0)
			.SetColumnSpacing(0)
			.AddClass("calendar__date")
			.Attach(GtkLabelHandle.New("Su").SetSizeRequest(40, 40).AddClass("calendar__date-header"), 0, 0, 1, 1)
			.Attach(GtkLabelHandle.New("Mo").AddClass("calendar__date-header"), 1, 0, 1, 1)
			.Attach(GtkLabelHandle.New("Tu").AddClass("calendar__date-header"), 2, 0, 1, 1)
			.Attach(GtkLabelHandle.New("We").AddClass("calendar__date-header"), 3, 0, 1, 1)
			.Attach(GtkLabelHandle.New("Th").AddClass("calendar__date-header"), 4, 0, 1, 1)
			.Attach(GtkLabelHandle.New("Fr").AddClass("calendar__date-header"), 5, 0, 1, 1)
			.Attach(GtkLabelHandle.New("Sa").AddClass("calendar__date-header"), 6, 0, 1, 1);

		var firstOfMonth = currentDateTime.AddDays(-currentDateTime.Day + 1);
		var startOfCalendar = firstOfMonth.AddDays(-(int)firstOfMonth.DayOfWeek);
		var current = startOfCalendar;

		for (var i = 1; i < 7; i++)
		{
			for (var j = 0; j < 7; j++)
			{
				var dayOfMonthLabel = GtkLabelHandle.New(current.Day.ToString());
				dayOfMonthLabel.AddClass(current.Month == firstOfMonth.Month ? "calendar__date--in-month" : "calendar__date--outside-month");
				if (current.Month == DateTime.Now.Month && current.Day == currentDateTime.Day) dayOfMonthLabel.AddClass("calendar__date--current-day");

				dateGrid.Attach(dayOfMonthLabel, j, i, 1, 1);
				current = current.AddDays(1);
			}
		}

		dateGrid.ShowAll();
		return dateGrid;
	}
}
