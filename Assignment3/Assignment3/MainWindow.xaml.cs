using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GeographyTools;
using Microsoft.EntityFrameworkCore;
using Windows.Devices.Geolocation;

namespace Assignment3
{
    public class Ticket
    {
        public int ID { get; set; }
        [Required]
        public Screening Screening { get; set; }
        [Required]
        public int ScreeningID { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime TimePurchased { get; set; }
    }
    public class Cinema
    {
        public int ID { get; set; }
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        [Required]
        [MaxLength(255)]
        public string City { get; set; }
    }
    public class Screening
    {
        public int ID { get; set; }
        [Column(TypeName = "time(0)")]
        public TimeSpan Time { get; set; }
        [Required]
        public Movie Movie { get; set; }
        [Required]
        public int MovieID { get; set; }
        [Required]
        public Cinema Cinema { get; set; }
        [Required]
        public int CinemaID { get; set; }
       
    }
    public class Movie
    {
        public int ID { get; set; }
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
        public Int16 Runtime { get; set; }
        [Column(TypeName = "date")]
        public DateTime ReleaseDate { get; set; }
        [Required]
        [MaxLength(255)]
        public string PosterPath { get; set; }
    }
    public class AppDbContext : DbContext
    {
        public DbSet<Cinema> Cinema { get; set; }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<Screening> Screening { get; set; }
        public DbSet<Movie> Movie { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(@"Data Source=(local)\SQLEXPRESS;Initial Catalog=DataAccessGUIAssignment;Integrated Security=True");
        }

        //Unique constraint on Name in Cinema
        protected override void OnModelCreating(ModelBuilder model)
        {
            model.Entity<Cinema>()
            .HasIndex(Cinema => Cinema.Name)
            .IsUnique();
}
    }
    public partial class MainWindow : Window
    {
        private Thickness spacing = new Thickness(5);
        private FontFamily mainFont = new FontFamily("Constantia");

        // Some GUI elements that we need to access in multiple methods.
        private ComboBox cityComboBox;
        private ListBox cinemaListBox;
        private StackPanel screeningPanel;
        private StackPanel ticketPanel;

        // An SQL connection that we will keep open for the entire program.

        private AppDbContext database = new();
        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        private void Start()
        {

            // Window options
            Title = "Cinemania";
            Width = 1000;
            Height = 600;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = Brushes.Black;

            // Main grid
            var grid = new Grid();
            Content = grid;
            grid.Margin = spacing;
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

            AddToGrid(grid, CreateCinemaGUI(), 0, 0);
            AddToGrid(grid, CreateScreeningGUI(), 0, 1);
            AddToGrid(grid, CreateTicketGUI(), 0, 2);

        }

        // Create the cinema part of the GUI: the left column.
        private UIElement CreateCinemaGUI()
        {
            var grid = new Grid
            {
                MinWidth = 200
            };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var title = new TextBlock
            {
                Text = "Select Cinema",
                FontFamily = mainFont,
                Foreground = Brushes.White,
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = spacing
            };
            AddToGrid(grid, title, 0, 0);

            // Create the dropdown of cities.
            cityComboBox = new ComboBox
            {
                Margin = spacing
            };
            foreach (string city in GetCities())
            {
                cityComboBox.Items.Add(city);
            }
            cityComboBox.SelectedIndex = 0;
            AddToGrid(grid, cityComboBox, 1, 0);

            // When we select a city, update the GUI with the Cinema in the currently selected city.
            cityComboBox.SelectionChanged += (sender, e) =>
            {
                UpdateCinemaList();
            };

            // Create the dropdown of Cinema.
            cinemaListBox = new ListBox
            {
                Margin = spacing
            };
            AddToGrid(grid, cinemaListBox, 2, 0);
            UpdateCinemaList();

            // When we select a cinema, update the GUI with the Screening in the currently selected cinema.
            cinemaListBox.SelectionChanged += (sender, e) =>
            {
                UpdateScreeningList();
            };

            return grid;
        }

        // Create the screening part of the GUI: the middle column.
        private UIElement CreateScreeningGUI()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var title = new TextBlock
            {
                Text = "Select Screening",
                FontFamily = mainFont,
                Foreground = Brushes.White,
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = spacing
            };
            AddToGrid(grid, title, 0, 0);

            var scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            AddToGrid(grid, scroll, 1, 0);

            screeningPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            scroll.Content = screeningPanel;

            UpdateScreeningList();

            return grid;
        }

        // Create the ticket part of the GUI: the right column.
        private UIElement CreateTicketGUI()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var title = new TextBlock
            {
                Text = "My Ticket",
                FontFamily = mainFont,
                Foreground = Brushes.White,
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = spacing
            };
            AddToGrid(grid, title, 0, 0);

            var scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            AddToGrid(grid, scroll, 1, 0);

            ticketPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            scroll.Content = ticketPanel;

            // Update the GUI with the initial list of Ticket.
            UpdateTicketList();

            return grid;
        }

        // Get a list of all cities that have Cinema in them.
        private IEnumerable<string> GetCities()
        {
            //create a list for the cities 
            var cities = new List<string>();
            //Add all unique cities to the list
            foreach (var city in database.Cinema.Select(c => c.City).Distinct())
            {
                cities.Add(city);
            }
            return cities;

        }

        // Get a list of all Cinema in the currently selected city.
        private IEnumerable<string> GetCinemasInSelectedCity()
        {
            string selectedCity = cityComboBox.SelectedItem.ToString();
            List<string> Cinema = database.Cinema.Where(c => c.City == selectedCity).Select(c => c.Name).ToList();
            return Cinema;
        }


        private void UpdateCinemaList()
        {
            cinemaListBox.Items.Clear();
            foreach (string cinema in GetCinemasInSelectedCity())
            {
                cinemaListBox.Items.Add(cinema);
            }
        }

        // Update the GUI with the Screening in the currently selected cinema.
        private void UpdateScreeningList()
        {
            screeningPanel.Children.Clear();
            if (cinemaListBox.SelectedIndex == -1)
            {
                return;
            }

            string selectedCinema = cinemaListBox.SelectedItem.ToString();
            List<Screening> screenings = database.Screening
                .Where(c => c.Cinema.Name == selectedCinema)
                .Include(s => s.Movie)
                .OrderBy(s => s.Time)
                .ToList();

            // For each screening:
            foreach (var screening in screenings)
            {
                // Create the button that will show all the info about the screening and let us buy a ticket for it.
                var button = new Button
                {
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Padding = spacing,
                    Cursor = Cursors.Hand,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                };
                screeningPanel.Children.Add(button);
                int screeningID = Convert.ToInt32(screening.ID);

                // When we click a screening, buy a ticket for it and update the GUI with the latest list of Ticket.
                button.Click += (sender, e) =>
                {
                    BuyTicket(screeningID);
                };

                // The rest of this method is just creating the GUI element for the screening.
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                button.Content = grid;

                var image = CreateImage(@"Posters\" + screening.Movie.PosterPath);
                image.Width = 50;
                image.Margin = spacing;
                image.ToolTip = new ToolTip { Content = screening.Movie.Title };
                AddToGrid(grid, image, 0, 0);
                Grid.SetRowSpan(image, 3);

                var time = (TimeSpan)screening.Time;
                var timeHeading = new TextBlock
                {
                    Text = TimeSpanToString(time),
                    Margin = spacing,
                    FontFamily = new FontFamily("Corbel"),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Yellow
                };
                AddToGrid(grid, timeHeading, 0, 1);

                var titleHeading = new TextBlock
                {
                    Text = Convert.ToString(screening.Movie.Title),
                    Margin = spacing,
                    FontFamily = mainFont,
                    FontSize = 16,
                    Foreground = Brushes.White,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                AddToGrid(grid, titleHeading, 1, 1);

                var releaseDate = Convert.ToDateTime(screening.Movie.ReleaseDate);
                int runtimeMinutes = Convert.ToInt32(screening.Movie.Runtime);
                var runtime = TimeSpan.FromMinutes(runtimeMinutes);
                string runtimeString = runtime.Hours + "h " + runtime.Minutes + "m";
                var details = new TextBlock
                {
                    Text = "📆 " + releaseDate.Year + "     ⏳ " + runtimeString,
                    Margin = spacing,
                    FontFamily = new FontFamily("Corbel"),
                    Foreground = Brushes.Silver
                };
                AddToGrid(grid, details, 2, 1);
            }
        }

        // Buy a ticket for the specified screening and update the GUI with the latest list of Ticket.
        private void BuyTicket(int screeningID)
        {
            int count = database.Ticket.Count(t => t.Screening.ID == screeningID);

            // If we don't, add it.
            if (count == 0)
            {
                Ticket ticket = new Ticket();
                ticket.Screening = database.Screening.First(s => s.ID == screeningID);
                ticket.TimePurchased = DateTime.Now;
                database.Add(ticket);
                database.SaveChanges();

                UpdateTicketList();
            }
        }

        // Update the GUI with the latest list of Ticket
        private void UpdateTicketList()
        {
            ticketPanel.Children.Clear();

            List<Ticket> tickets = database.Ticket
                .Include(t => t.Screening)
                .Include(t => t.Screening.Movie)
                .Include(t => t.Screening.Cinema)
                .OrderBy(t => t.TimePurchased)
                .ToList();


            // For each ticket:
            foreach (var ticket in tickets)
            {
                // Create the button that will show all the info about the ticket and let us remove it.
                var button = new Button
                {
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Padding = spacing,
                    Cursor = Cursors.Hand,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                };
                ticketPanel.Children.Add(button);
                int ticketID = Convert.ToInt32(ticket.ID);

                // When we click a ticket, remove it and update the GUI with the latest list of Ticket.
                button.Click += (sender, e) =>
                {
                    RemoveTicket(ticketID);
                };

                // The rest of this method is just creating the GUI element for the screening.
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                button.Content = grid;

                var image = CreateImage(@"Posters\" + ticket.Screening.Movie.PosterPath);
                image.Width = 30;
                image.Margin = spacing;
                image.ToolTip = new ToolTip { Content = ticket.Screening.Movie.Title };
                AddToGrid(grid, image, 0, 0);
                Grid.SetRowSpan(image, 2);

                var titleHeading = new TextBlock
                {
                    Text = Convert.ToString(ticket.Screening.Movie.Title),
                    Margin = spacing,
                    FontFamily = mainFont,
                    FontSize = 14,
                    Foreground = Brushes.White,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                AddToGrid(grid, titleHeading, 0, 1);

                var time = (TimeSpan)ticket.Screening.Time;
                string timeString = TimeSpanToString(time);
                var timeAndCinemaHeading = new TextBlock
                {
                    Text = timeString + " - " + ticket.Screening.Cinema.Name,
                    Margin = spacing,
                    FontFamily = new FontFamily("Corbel"),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Yellow,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                AddToGrid(grid, timeAndCinemaHeading, 1, 1);
            }
        }

        // Remove the ticket for the specified screening and update the GUI with the latest list of Ticket.
        private void RemoveTicket(int ticketID)
        {

            database.Remove(database.Ticket.First(t => t.ID == ticketID));
            database.SaveChanges();


            UpdateTicketList();
        }

        // Helper method to add a GUI element to the specified row and column in a grid.
        private void AddToGrid(Grid grid, UIElement element, int row, int column)
        {
            grid.Children.Add(element);
            Grid.SetRow(element, row);
            Grid.SetColumn(element, column);
        }

        // Helper method to create a high-quality image for the GUI.
        private Image CreateImage(string filePath)
        {
            ImageSource source = new BitmapImage(new Uri(filePath, UriKind.RelativeOrAbsolute));
            Image image = new Image
            {
                Source = source,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
            return image;
        }

        // Helper method to turn a TimeSpan object into a string, such as 2:05.
        private string TimeSpanToString(TimeSpan timeSpan)
        {
            string hourString = timeSpan.Hours.ToString().PadLeft(2, '0');
            string minuteString = timeSpan.Minutes.ToString().PadLeft(2, '0');
            string timeString = hourString + ":" + minuteString;
            return timeString;
        }
    }
}
