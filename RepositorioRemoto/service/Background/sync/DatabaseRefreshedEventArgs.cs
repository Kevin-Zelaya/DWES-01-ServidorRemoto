public class DatabaseRefreshedEventArgs : EventArgs
{
    public int RegistrosCargados { get; }
    public DateTime dateTime { get; }

    public DatabaseRefreshedEventArgs(int registrosCargados, DateTime dateTime)
    {
        RegistrosCargados = registrosCargados;
        this.dateTime = dateTime;
    }
}