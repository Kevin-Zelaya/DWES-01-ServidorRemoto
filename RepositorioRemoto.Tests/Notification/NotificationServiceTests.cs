using NUnit.Framework;

[TestFixture]
public class NotificationServiceTests
{
    private NotificationService _notificationService = null!;

    [SetUp]
    public void Setup()
    {
        _notificationService = new NotificationService();
    }

    [Test]
    public void NotifyUserCreated_ShouldTriggerEvent()
    {
        // Arrange
        var user = new UserModel { id = 1, name = "Kevin", username = "kevin" };
        UserModel? receivedUser = null;
        
        _notificationService.UserCreated += (sender, u) => receivedUser = u;

        // Act
        _notificationService.NotifyUserCreated(user);

        // Assert
        Assert.That(receivedUser, Is.Not.Null);
        Assert.That(receivedUser?.id, Is.EqualTo(1));
        Assert.That(receivedUser?.name, Is.EqualTo("Kevin"));
    }

    [Test]
    public void NotifyUserUpdated_ShouldTriggerEvent()
    {
        // Arrange
        var user = new UserModel { id = 2, name = "Juan", username = "juan" };
        UserModel? receivedUser = null;
        
        _notificationService.UserUpdated += (sender, u) => receivedUser = u;

        // Act
        _notificationService.NotifyUserUpdated(user);

        // Assert
        Assert.That(receivedUser, Is.Not.Null);
        Assert.That(receivedUser?.id, Is.EqualTo(2));
        Assert.That(receivedUser?.name, Is.EqualTo("Juan"));
    }

    [Test]
    public void NotifyUserDeleted_ShouldTriggerEvent()
    {
        // Arrange
        int? receivedId = null;
        
        _notificationService.UserDeleted += (sender, id) => receivedId = id;

        // Act
        _notificationService.NotifyUserDeleted(42);

        // Assert
        Assert.That(receivedId, Is.Not.Null);
        Assert.That(receivedId, Is.EqualTo(42));
    }
}