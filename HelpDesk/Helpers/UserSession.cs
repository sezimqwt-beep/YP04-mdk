using HelpDesk.Models;

namespace HelpDesk.Helpers
{
    public static class UserSession
    {
        public static User CurrentUser { get; private set; }

        public static void Start(User user)
        {
            CurrentUser = user;
        }

        public static void End()
        {
            CurrentUser = null;
        }
    }
}