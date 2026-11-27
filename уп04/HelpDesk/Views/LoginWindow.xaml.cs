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
            Loaded += (s, e) => HelpDesk.Helpers.DemoRunner.Run(this);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordInput.Password;

            if (login == "admin" && password == "12345")
            {
                MainWindow mainWindow = new MainWindow();

                mainWindow.Show();

                this.Close();
            }
            else
            {
                MessageBox.Show(
                    "Неверный логин или пароль.",
                    "Ошибка авторизации",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
