using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using PersonalJournalDesktopApp.Models;

namespace PersonalJournalDesktopApp.Data
{

    public class DatabaseService
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public DatabaseService()
        {
            _dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
            _connectionString = $"Data Source={_dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Create JournalEntries table
            var createEntriesTable = connection.CreateCommand();
            createEntriesTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS JournalEntries (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Date TEXT NOT NULL UNIQUE,
                Title TEXT NOT NULL,
                Content TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                PrimaryMoodId INTEGER,
                SecondaryMood1Id INTEGER,
                SecondaryMood2Id INTEGER
            )";
            createEntriesTable.ExecuteNonQuery();

            // Create Moods table
            var createMoodsTable = connection.CreateCommand();
            createMoodsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Moods (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Category INTEGER NOT NULL,
                Emoji TEXT NOT NULL
            )";
            createMoodsTable.ExecuteNonQuery();

            // Create Settings table
            var createSettingsTable = connection.CreateCommand();
            createSettingsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Settings (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL
            )";
            createSettingsTable.ExecuteNonQuery();

            // Seed default moods
            SeedDefaultMoods(connection);
        }

        private void SeedDefaultMoods(SqliteConnection connection)
        {
            var checkMoods = connection.CreateCommand();
            checkMoods.CommandText = "SELECT COUNT(*) FROM Moods";
            var count = Convert.ToInt32(checkMoods.ExecuteScalar());

            if (count == 0)
            {
                var moods = Mood.GetDefaultMoods();
                foreach (var mood in moods)
                {
                    var insert = connection.CreateCommand();
                    insert.CommandText = @"
                    INSERT INTO Moods (Id, Name, Category, Emoji)
                    VALUES (@Id, @Name, @Category, @Emoji)";
                    insert.Parameters.AddWithValue("@Id", mood.Id);
                    insert.Parameters.AddWithValue("@Name", mood.Name);
                    insert.Parameters.AddWithValue("@Category", (int)mood.Category);
                    insert.Parameters.AddWithValue("@Emoji", mood.Emoji);
                    insert.ExecuteNonQuery();
                }
            }
        }

        // ==================== JOURNAL ENTRY CRUD ====================

        public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
        {
            return await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                SELECT * FROM JournalEntries 
                WHERE Date = @Date";
                command.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new JournalEntry
                    {
                        Id = reader.GetInt32(0),
                        Date = DateTime.Parse(reader.GetString(1)),
                        Title = reader.GetString(2),
                        Content = reader.GetString(3),
                        CreatedAt = DateTime.Parse(reader.GetString(4)),
                        UpdatedAt = DateTime.Parse(reader.GetString(5)),
                        PrimaryMoodId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                        SecondaryMood1Id = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                        SecondaryMood2Id = reader.IsDBNull(8) ? null : reader.GetInt32(8)
                    };
                }
                return null;
            });
        }

        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            return await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM JournalEntries ORDER BY Date DESC";

                var entries = new List<JournalEntry>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    entries.Add(new JournalEntry
                    {
                        Id = reader.GetInt32(0),
                        Date = DateTime.Parse(reader.GetString(1)),
                        Title = reader.GetString(2),
                        Content = reader.GetString(3),
                        CreatedAt = DateTime.Parse(reader.GetString(4)),
                        UpdatedAt = DateTime.Parse(reader.GetString(5)),
                        PrimaryMoodId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                        SecondaryMood1Id = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                        SecondaryMood2Id = reader.IsDBNull(8) ? null : reader.GetInt32(8)
                    });
                }
                return entries;
            });
        }

        public async Task<int> SaveEntryAsync(JournalEntry entry)
        {
            return await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                if (entry.Id == 0)
                {
                    // CREATE new entry
                    command.CommandText = @"
                    INSERT INTO JournalEntries 
                    (Date, Title, Content, CreatedAt, UpdatedAt, PrimaryMoodId, SecondaryMood1Id, SecondaryMood2Id)
                    VALUES (@Date, @Title, @Content, @CreatedAt, @UpdatedAt, @PrimaryMoodId, @SecondaryMood1Id, @SecondaryMood2Id);
                    SELECT last_insert_rowid();";
                    command.Parameters.AddWithValue("@Date", entry.Date.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@Title", entry.Title);
                    command.Parameters.AddWithValue("@Content", entry.Content);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("o"));
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("o"));
                    command.Parameters.AddWithValue("@PrimaryMoodId", (object?)entry.PrimaryMoodId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SecondaryMood1Id", (object?)entry.SecondaryMood1Id ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SecondaryMood2Id", (object?)entry.SecondaryMood2Id ?? DBNull.Value);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
                else
                {
                    // UPDATE existing entry
                    command.CommandText = @"
                    UPDATE JournalEntries 
                    SET Title = @Title, Content = @Content, UpdatedAt = @UpdatedAt,
                        PrimaryMoodId = @PrimaryMoodId, SecondaryMood1Id = @SecondaryMood1Id, 
                        SecondaryMood2Id = @SecondaryMood2Id
                    WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", entry.Id);
                    command.Parameters.AddWithValue("@Title", entry.Title);
                    command.Parameters.AddWithValue("@Content", entry.Content);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("o"));
                    command.Parameters.AddWithValue("@PrimaryMoodId", (object?)entry.PrimaryMoodId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SecondaryMood1Id", (object?)entry.SecondaryMood1Id ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SecondaryMood2Id", (object?)entry.SecondaryMood2Id ?? DBNull.Value);

                    command.ExecuteNonQuery();
                    return entry.Id;
                }
            });
        }

        public async Task<bool> DeleteEntryAsync(int id)
        {
            return await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM JournalEntries WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);

                return command.ExecuteNonQuery() > 0;
            });
        }

        // ==================== MOOD OPERATIONS ====================

        public async Task<List<Mood>> GetAllMoodsAsync()
        {
            return await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Moods ORDER BY Category, Name";

                var moods = new List<Mood>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    moods.Add(new Mood
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Category = (MoodCategory)reader.GetInt32(2),
                        Emoji = reader.GetString(3)
                    });
                }
                return moods;
            });
        }

        public async Task<Mood?> GetMoodByIdAsync(int id)
        {
            return await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Moods WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new Mood
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Category = (MoodCategory)reader.GetInt32(2),
                        Emoji = reader.GetString(3)
                    };
                }
                return null;
            });
        }

        // ==================== SETTINGS (THEME) ====================

        public async Task<string?> GetSettingAsync(string key)
        {
            return await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT Value FROM Settings WHERE Key = @Key";
                command.Parameters.AddWithValue("@Key", key);

                var result = command.ExecuteScalar();
                return result?.ToString();
            });
        }

        public async Task SaveSettingAsync(string key, string value)
        {
            await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                INSERT INTO Settings (Key, Value) VALUES (@Key, @Value)
                ON CONFLICT(Key) DO UPDATE SET Value = @Value";
                command.Parameters.AddWithValue("@Key", key);
                command.Parameters.AddWithValue("@Value", value);
                command.ExecuteNonQuery();
            });
        }
    }
}

