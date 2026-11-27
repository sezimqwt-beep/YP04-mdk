using ClassroomBooking.Models;
using ClassroomBooking.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClassroomBooking
{
    public partial class MainWindow : Window
    {
        private readonly UserService userService = new UserService();
        private readonly RoomService roomService = new RoomService();
        private readonly BookingService bookingService = new BookingService();

        public MainWindow()
        {
            InitializeComponent();

            UserComboBox.ItemsSource = userService.GetUsers();
            RoomComboBox.ItemsSource = roomService.GetRooms();
            BookingDatePicker.SelectedDate = DateTime.Today;

            RefreshBookings();
            Loaded += (s, e) => DemoRunner.Run(this);
        }

        // Члены для автоматической съёмки скриншотов (см. DemoRunner.cs), в обычной работе не используются
        public void SetInput(int userId, string roomNumber, DateTime date, string start, string end)
        {
            UserComboBox.SelectedItem = userService.GetUsers().First(u => u.Id == userId);
            RoomComboBox.SelectedItem = roomService.GetRooms().First(r => r.Number == roomNumber);
            BookingDatePicker.SelectedDate = date;
            StartTimeTextBox.Text = start;
            EndTimeTextBox.Text = end;
        }
        public void Click() => BookButton_Click(null, null);
        public string Status => StatusTextBlock.Text;
        public int BookingsCount => bookingService.GetBookings().Count;

        private void BookButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(UserComboBox.SelectedItem is User user))
            {
                StatusTextBlock.Text = "Выберите пользователя.";
                return;
            }

            if (!(RoomComboBox.SelectedItem is Room room))
            {
                StatusTextBlock.Text = "Выберите аудиторию.";
                return;
            }

            if (!(BookingDatePicker.SelectedDate is DateTime date))
            {
                StatusTextBlock.Text = "Выберите дату.";
                return;
            }

            if (!TimeSpan.TryParse(StartTimeTextBox.Text, out var startTime) ||
                !TimeSpan.TryParse(EndTimeTextBox.Text, out var endTime))
            {
                StatusTextBlock.Text = "Введите время в формате ЧЧ:ММ.";
                return;
            }

            if (startTime >= endTime)
            {
                StatusTextBlock.Text = "Время начала должно быть раньше времени окончания.";
                return;
            }

            if (!room.IsAvailable)
            {
                StatusTextBlock.Text = "Выбранная аудитория недоступна для бронирования.";
                return;
            }

            var booking = new Booking
            {
                UserId = user.Id,
                RoomId = room.Id,
                Date = date,
                StartTime = startTime,
                EndTime = endTime
            };

            if (!bookingService.AddBooking(booking))
            {
                StatusTextBlock.Text = "На это время аудитория уже забронирована.";
                return;
            }

            StatusTextBlock.Text = "Бронирование создано.";
            RefreshBookings();
        }

        private void RefreshBookings()
        {
            var rooms = roomService.GetRooms();
            var users = userService.GetUsers();

            var items = bookingService.GetBookings()
                .Select(x =>
                {
                    var user = users.FirstOrDefault(u => u.Id == x.UserId);
                    var room = rooms.FirstOrDefault(r => r.Id == x.RoomId);

                    return $"{x.Date:dd.MM.yyyy} | {x.StartTime:hh\\:mm}-{x.EndTime:hh\\:mm} | " +
                           $"{room?.Number} | {user?.Name}";
                })
                .ToList();

            BookingsListBox.ItemsSource = items;
        }
    }
}
