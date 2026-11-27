using HelpDesk.Data;
using HelpDesk.Helpers;
using HelpDesk.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;

using System.Windows.Input;

namespace HelpDesk.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Ticket> tickets = new List<Ticket>();

        public MainWindow()
        {
            InitializeComponent();

            DateTextBlock.Text = "Сегодня: " + DateTime.Today.ToString("dd.MM.yyyy");

            SafeLoadTickets();
            LoadStatuses();
            ShowCurrentUser();
            ApplyRolePermissions();
        }

        private void ShowCurrentUser()
        {
            if (UserSession.CurrentUser == null)
            {
                CurrentUserTextBlock.Text = "Пользователь: не определён";
                return;
            }

            CurrentUserTextBlock.Text =
                "Пользователь: " +
                UserSession.CurrentUser.FullName +
                " (" + UserSession.CurrentUser.RoleName + ")";
        }

        private void ApplyRolePermissions()
        {
            if (UserSession.CurrentUser == null)
                return;

            bool isAdmin =
                UserSession.CurrentUser.RoleName == "Администратор";

            // Учебное правило: удалять заявки может только администратор.
            DeleteTicketButton.IsEnabled = isAdmin;
        }

        private void LoadTickets()
        {
            tickets = new List<Ticket>();

            string sql = @"
                SELECT
                    t.Id,
                    t.UserId,
                    t.CategoryId,
                    c.Name AS Category,
                    t.Description,
                    t.PriorityId,
                    p.Name AS Priority,
                    t.StatusId,
                    s.Name AS Status,
                    t.AssignedUserId,
                    t.CreatedAt,
                    t.UpdatedAt
                FROM Tickets t
                INNER JOIN Categories c ON t.CategoryId = c.Id
                INNER JOIN Priorities p ON t.PriorityId = p.Id
                INNER JOIN Statuses s ON t.StatusId = s.Id
                ORDER BY t.Id DESC;";

            using (SqlConnection connection = Database.GetConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Ticket ticket = new Ticket
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            UserId = Convert.ToInt32(reader["UserId"]),
                            CategoryId = Convert.ToInt32(reader["CategoryId"]),
                            Category = reader["Category"].ToString(),
                            Description = reader["Description"].ToString(),
                            PriorityId = Convert.ToInt32(reader["PriorityId"]),
                            Priority = reader["Priority"].ToString(),
                            StatusId = Convert.ToInt32(reader["StatusId"]),
                            Status = reader["Status"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        };

                        if (reader["AssignedUserId"] != DBNull.Value)
                            ticket.AssignedUserId = Convert.ToInt32(reader["AssignedUserId"]);

                        if (reader["UpdatedAt"] != DBNull.Value)
                            ticket.UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"]);

                        tickets.Add(ticket);
                    }
                }
            }

            ShowTickets(tickets);
        }

        private void SafeLoadTickets()
        {
            try
            {
                LoadTickets();
            }
            catch (SqlException ex)
            {
                TicketsDataGrid.ItemsSource = null;

                MessageBox.Show(
                    "Не удалось загрузить заявки из базы данных.\n\n" +
                    "Проверьте SQL Server, имя базы и строку подключения.\n\n" +
                    ex.Message,
                    "Ошибка базы данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadStatuses()
        {
            StatusComboBox.Items.Clear();
            StatusComboBox.Items.Add("Все");

            string sql = "SELECT Name FROM Statuses ORDER BY Id";

            try
            {
                using (SqlConnection connection = Database.GetConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            StatusComboBox.Items.Add(reader["Name"].ToString());
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Не удалось загрузить статусы:\n" + ex.Message, "Ошибка базы данных",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            StatusComboBox.SelectedIndex = 0;
        }

        private void ShowTickets(List<Ticket> list)
        {
            TicketsDataGrid.ItemsSource = list;
            CountTextBlock.Text = "Показано заявок: " + list.Count + " из " + tickets.Count;
        }

        private void ApplyFilter()
        {
            string searchText = SearchTextBox.Text.Trim().ToLower();
            string status = StatusComboBox.SelectedItem as string ?? "Все";

            var result = tickets
                .Where(ticket =>
                    ticket.Description.ToLower().Contains(searchText) ||
                    ticket.Category.ToLower().Contains(searchText) ||
                    ticket.Status.ToLower().Contains(searchText))
                .Where(ticket => status == "Все" || ticket.Status == status)
                .ToList();

            ShowTickets(result);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ApplyFilter();
        }

        private void ResetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            StatusComboBox.SelectedIndex = 0;
            ShowTickets(tickets);
        }

        private void AddTicketButton_Click(object sender, RoutedEventArgs e)
        {
            TicketCreateWindow window = new TicketCreateWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void EditTicketButton_Click(object sender, RoutedEventArgs e)
        {
            if (TicketsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Сначала выберите заявку.", "Редактирование",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Ticket selectedTicket = TicketsDataGrid.SelectedItem as Ticket;

            if (selectedTicket == null)
            {
                MessageBox.Show("Не удалось получить данные выбранной заявки.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TicketEditWindow window = new TicketEditWindow(selectedTicket);
            window.Owner = this;

            bool? result = window.ShowDialog();

            if (result == true)
            {
                TicketsDataGrid.Items.Refresh();
            }
        }

        private void TicketsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TicketsDataGrid.SelectedItem != null)
                EditTicketButton_Click(sender, e);
        }

        private void DeleteTicketButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем наличие выбранной строки
            if (TicketsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите заявку для удаления.", "Удаление",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Просим подтверждение
            MessageBoxResult result = MessageBox.Show(
                "Вы действительно хотите удалить выбранную заявку?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show(
                    "На следующих занятиях здесь будет выполняться " +
                    "удаление записи из MS SQL Server.",
                    "HelpDesk",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Выйти из учётной записи?",
                "HelpDesk",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            UserSession.End();

            LoginWindow loginWindow = new LoginWindow();

            loginWindow.Show();

            Close();
        }
    }
}