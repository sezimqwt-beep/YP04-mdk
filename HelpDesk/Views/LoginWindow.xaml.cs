using HelpDesk.Helpers;
using HelpDesk.Models;
using HelpDesk.Services;
using System.Data.SqlClient;
using System.Windows;

namespace HelpDesk.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordInput.Password;

            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Введите логин.");
                LoginTextBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите пароль.");
                PasswordInput.Focus();
                return;
            }

            try
            {
                AuthService authService = new AuthService();
                User user = authService.Authenticate(login, password);

                if (user == null)
                {
                    MessageBox.Show(
                        "Неверный логин или пароль, либо учётная запись отключена.",
                        "Авторизация",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    PasswordInput.Clear();
                    PasswordInput.Focus();
                    return;
                }

                UserSession.Start(user);
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "Не удалось обратиться к базе данных:\n" + ex.Message,
                    "Ошибка SQL Server",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}