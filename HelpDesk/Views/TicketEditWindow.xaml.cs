using HelpDesk.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HelpDesk.Views
{
    /// <summary>
    /// Логика взаимодействия для TicketEditWindow.xaml
    /// </summary>
    public partial class TicketEditWindow : Window
    {
        private readonly Ticket ticket;

        public TicketEditWindow(Ticket ticket)
        {
            InitializeComponent();

            this.ticket = ticket;
            TicketNumberTextBlock.Text = "Заявка №: " + ticket.Id;
            DescriptionTextBox.Text = ticket.Description;
            Select(CategoryComboBox, ticket.Category);
            Select(PriorityComboBox, ticket.Priority);
            Select(StatusComboBox, ticket.Status);
        }

        private static void Select(ComboBox box, string value)
        {
            box.SelectedItem = box.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(i => i.Content.ToString() == value);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem == null ||
                PriorityComboBox.SelectedItem == null ||
                StatusComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Заполните все поля заявки.", "Проверка данных",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ticket.Category = ((ComboBoxItem)CategoryComboBox.SelectedItem).Content.ToString();
            ticket.Description = DescriptionTextBox.Text.Trim();
            ticket.Priority = ((ComboBoxItem)PriorityComboBox.SelectedItem).Content.ToString();
            ticket.Status = ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString();

            DialogResult = true;
        }
    }
}