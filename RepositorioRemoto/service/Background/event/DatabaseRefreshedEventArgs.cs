public class DatabaseRefreshedEventArgs : EventArgs
{
    public bool RegistrosCargados { get; }
    public DateTime dateTime { get; }

    public DatabaseRefreshedEventArgs(bool registrosCargados, DateTime dateTime)
    {
        RegistrosCargados = registrosCargados;
        this.dateTime = dateTime;
    }
}