public class NotificationService : INotificationService
{
    public event EventHandler<UserModel>? UserCreated;
    public event EventHandler<UserModel>? UserUpdated;
    public event EventHandler<int>? UserDeleted;

    public void NotifyUserCreated(UserModel user)
    {
        UserCreated?.Invoke(this, user);
    }

    public void NotifyUserUpdated(UserModel user)
    {
        UserUpdated?.Invoke(this, user);
    }

    public void NotifyUserDeleted(int userId)
    {
        UserDeleted?.Invoke(this, userId);
    }
}