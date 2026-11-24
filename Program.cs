using Microsoft.Data.Sqlite;

namespace CatalogoVideojuegos;

class Program
{
    // Cadena de conexión: indica el archivo de la base de datos SQLite.
    // Si el archivo no existe, SQLite lo crea al abrir la conexión.
    private const string CadenaConexion = "Data Source=videojuegos.db";

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== Catálogo de videojuegos (ADO.NET + SQLite) ===\n");

        CrearTabla();
        LimpiarTabla();      // Evita duplicados si se ejecuta el programa varias veces
        InsertarJuegos();
        MostrarJuegos();

        Console.WriteLine("\nFin del programa.");
    }

    /// <summary>Crea la base de datos (si no existe) y la tabla Videojuegos.</summary>
    static void CrearTabla()
    {
        using var conexion = new SqliteConnection(CadenaConexion);
        conexion.Open();

        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            CREATE TABLE IF NOT EXISTS Videojuegos (
                Id     INTEGER PRIMARY KEY AUTOINCREMENT,
                Titulo TEXT    NOT NULL,
                Genero TEXT    NOT NULL,
                Anio   INTEGER NOT NULL
            );
            """;
        comando.ExecuteNonQuery();

        Console.WriteLine("[1] Base de datos y tabla 'Videojuegos' listas.");
    }

    /// <summary>Borra los registros anteriores para empezar siempre desde cero.</summary>
    static void LimpiarTabla()
    {
        using var conexion = new SqliteConnection(CadenaConexion);
        conexion.Open();

        using var comando = conexion.CreateCommand();
        comando.CommandText = "DELETE FROM Videojuegos; DELETE FROM sqlite_sequence WHERE name = 'Videojuegos';";
        comando.ExecuteNonQuery();
    }

    /// <summary>Inserta tres registros usando parámetros (evita inyección SQL).</summary>
    static void InsertarJuegos()
    {
        var juegos = new[]
        {
            (Titulo: "The Legend of Zelda: Breath of the Wild", Genero: "Aventura", Anio: 2017),
            (Titulo: "Minecraft",                               Genero: "Sandbox",  Anio: 2011),
            (Titulo: "Hollow Knight",                           Genero: "Metroidvania", Anio: 2017)
        };

        using var conexion = new SqliteConnection(CadenaConexion);
        conexion.Open();

        foreach (var juego in juegos)
        {
            using var comando = conexion.CreateCommand();
            comando.CommandText =
                "INSERT INTO Videojuegos (Titulo, Genero, Anio) VALUES ($titulo, $genero, $anio);";
            comando.Parameters.AddWithValue("$titulo", juego.Titulo);
            comando.Parameters.AddWithValue("$genero", juego.Genero);
            comando.Parameters.AddWithValue("$anio", juego.Anio);
            comando.ExecuteNonQuery();
        }

        Console.WriteLine($"[2] Se insertaron {juegos.Length} registros.\n");
    }

    /// <summary>Consulta la tabla y muestra cada fila con un SqliteDataReader.</summary>
    static void MostrarJuegos()
    {
        using var conexion = new SqliteConnection(CadenaConexion);
        conexion.Open();

        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT Id, Titulo, Genero, Anio FROM Videojuegos ORDER BY Id;";

        using var lector = comando.ExecuteReader();

        Console.WriteLine("[3] Registros almacenados:");
        Console.WriteLine($"{"Id",-4}{"Título",-45}{"Género",-15}{"Año",-6}");
        Console.WriteLine(new string('-', 70));

        while (lector.Read())
        {
            int id = lector.GetInt32(0);
            string titulo = lector.GetString(1);
            string genero = lector.GetString(2);
            int anio = lector.GetInt32(3);

            Console.WriteLine($"{id,-4}{titulo,-45}{genero,-15}{anio,-6}");
        }
    }
}
