using System.Reactive.Linq;
using System.Reactive.Subjects;
using GLib;
using Glimpse.Common.Gtk;
using Glimpse.Common.System.Reactive;
using Gtk;
using Microsoft.Extensions.DependencyInjection;
using DateTime = System.DateTime;
using ReactiveMarbles.ObservableEvents;

namespace Glimpse.SidePane.Components.Calendar;

public class CalendarWindow : Bin
{
	public CalendarWindow([FromKeyedServices(Timers.OneSecond)] IObservable<DateTime> oneSecondTimer)
	{
		Expand = false;

		var displayedDateTimeObs = new BehaviorSubject<DateTime>(DateTime.Now);

		var todayLabel = new Label();
		todayLabel.Halign = Align.Fill;
		todayLabel.Xalign = 0;
		todayLabel.HeightRequest = 50;
		todayLabel.AddClass("calendar__today");

		oneSecondTimer
			.DistinctUntilChanged(x => x.Date)
			.ObserveOn(new GLibSynchronizationContext())
			.Subscribe(dt => todayLabel.Text = dt.ToString("dddd, MMMM dd"));

		var monthLabel = new Label();
		monthLabel.Expand = true;
		monthLabel.Xalign = 0;

		var monthUpButton = new Button();
		monthUpButton.AddButtonStates();
		monthUpButton.Image = new Image { IconName = "go-up-symbolic", PixelSize = 16 };
		monthUpButton.ObserveButtonRelease().WithLatestFrom(displayedDateTimeObs).Subscribe(t => displayedDateTimeObs.OnNext(t.Second.AddMonths(-1)));

		var monthDownButton = new Button();
		monthDownButton.AddButtonStates();
		monthDownButton.Image = new Image { IconName = "go-down-symbolic", PixelSize = 16 };
		monthDownButton.ObserveButtonRelease().WithLatestFrom(displayedDateTimeObs).Subscribe(t => displayedDateTimeObs.OnNext(t.Second.AddMonths(1)));

		var monthSelectorLayout = new Box(Orientation.Horizontal, 0);
		monthSelectorLayout.Add(monthLabel);
		monthSelectorLayout.Add(monthUpButton);
		monthSelectorLayout.Add(monthDownButton);
		monthSelectorLayout.AddClass("calendar__month");

		var layout = new Box(Orientation.Vertical, 0);
		layout.Expand = true;
		layout.Halign = Align.Fill;
		layout.Valign = Align.Fill;
		layout.AddClass("calendar__layout");
		layout.Add(todayLabel);
		layout.Add(monthSelectorLayout);
		Add(layout);

		Grid currentDateTimeGrid = null;

		displayedDateTimeObs.Subscribe(dt =>
		{
			monthLabel.Text = dt.ToString("MMMM yyyy");
			if (currentDateTimeGrid != null) currentDateTimeGrid.Destroy();
			currentDateTimeGrid = CreateDateGrid(dt);
			layout.Add(currentDateTimeGrid);
		});

		this.ObserveEvent(w => w.Events().Mapped).Subscribe(_ =>
		{
			displayedDateTimeObs.OnNext(DateTime.Now);
		});
	}

	private Grid CreateDateGrid(DateTime currentDateTime)
	{
		var dateGrid = new Grid();
		dateGrid.ColumnHomogeneous = true;
		dateGrid.RowHomogeneous = true;
		dateGrid.RowSpacing = 0;
		dateGrid.ColumnSpacing = 0;
		dateGrid.Attach(new Label("Su") { WidthRequest = 40, HeightRequest = 40 }.AddClass("calendar__date-header"), 0, 0, 1, 1);
		dateGrid.Attach(new Label("Mo").AddClass("calendar__date-header"), 1, 0, 1, 1);
		dateGrid.Attach(new Label("Tu").AddClass("calendar__date-header"), 2, 0, 1, 1);
		dateGrid.Attach(new Label("We").AddClass("calendar__date-header"), 3, 0, 1, 1);
		dateGrid.Attach(new Label("Th").AddClass("calendar__date-header"), 4, 0, 1, 1);
		dateGrid.Attach(new Label("Fr").AddClass("calendar__date-header"), 5, 0, 1, 1);
		dateGrid.Attach(new Label("Sa").AddClass("calendar__date-header"), 6, 0, 1, 1);
		dateGrid.AddClass("calendar__date");

		var firstOfMonth = currentDateTime.AddDays(-currentDateTime.Day + 1);
		var startOfCalendar = firstOfMonth.AddDays(-(int)firstOfMonth.DayOfWeek);
		var current = startOfCalendar;

		for (var i = 1; i < 7; i++)
		{
			for (var j = 0; j < 7; j++)
			{
				var dayOfMonthLabel = new Label(current.Day.ToString());
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
