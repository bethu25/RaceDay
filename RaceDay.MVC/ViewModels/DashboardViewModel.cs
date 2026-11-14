namespace RaceDay.MVC.ViewModels
{
    // Stores the information displayed on the organiser dashboard.
    public class DashboardViewModel
    {
        // Stores the total number of events created by the organiser.
        public int TotalEvents { get; set; }

        // Stores the total number of participant enrolments.
        public int TotalEnrolments { get; set; }

        // Stores the number of upcoming events.
        public int UpcomingEvents { get; set; }

        // Stores the event information displayed in the dashboard table.
        public List<DashboardEventViewModel> Events { get; set; }
            = new List<DashboardEventViewModel>();
    }

    // Stores the details of an individual event.
    public class DashboardEventViewModel
    {
        public int EventID { get; set; }

        public string EventName { get; set; } = "";

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = "";

        public int TotalEnrolments { get; set; }
    }
}