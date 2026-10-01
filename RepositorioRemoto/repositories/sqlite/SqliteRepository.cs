using Microsoft.Data.Sqlite;

public class SqliteRepository 
{
    /*
    private readonly string _connectionString;
    private const string CREATE_QUERY = "CREATE TABLE IF NOT EXISTS Contacs (id INTEGER PRIMARY KEY, user_id INTEGER, name TEXT, lastname TEXT, alias TEXT, email TEXT, phone_number TEXT, UNIQUE(phone_number, user_id))";

    public SqliteRepository(string connectionString)
    {
        _connectionString = GetDatabasePath;

        CreateDatabaseAsync();
    }
    public void CreateDatabaseAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = new SqliteCommand(CREATE_QUERY, connection);
        command.ExecuteNonQuery();
    }
    private string GetDatabasePath()
    {
        String projectPath = AppDomain.CurrentDomain.BaseDirectory; // Ruta del archivo actual
        DirectoryInfo? projectInfo = Directory.GetParent(projectPath)? // Obtener direcotrios padre
            .Parent?
            .Parent?
            .Parent;

        if (projectInfo == null) 
        {
            System.Console.WriteLine($"Error en la ruta contenedora: {projectInfo.FullName}");
        }
        // Buscar directorio repository
        String repositoryPath = Path.Combine(projectInfo.FullName, "Repository");

        if (!Directory.Exists(repositoryPath)) // Comprobar si repository existe
        {
            System.Console.WriteLine($"No existe el directorio: {repositoryPath}");
        }
        
        String databasePath = Path.Combine(repositoryPath, "database.db"); // crear ruta con el archivo de la base de datos
        
        return $"Data Source={databasePath}"; // retorna algo parecido a ./repository/database.db
    }
    */
}