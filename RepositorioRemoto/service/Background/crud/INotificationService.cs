public interface INotificationService
{
    event EventHandler<UserModel>? UserCreated;
    event EventHandler<UserModel>? UserUpdated;
    event EventHandler<int>? UserDeleted;

    void NotifyUserCreated(UserModel user);
    void NotifyUserUpdated(UserModel user);
    void NotifyUserDeleted(int userId);
}