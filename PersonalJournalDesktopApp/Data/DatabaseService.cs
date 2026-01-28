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
            try
            {
                _dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
                _connectionString = $"Data Source={_dbPath}";
                InitializeDatabase();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                throw;
            }
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
                SecondaryMood2Id INTEGER,
                CategoryId INTEGER
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

            // Create Tags table
            var createTagsTable = connection.CreateCommand();
            createTagsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Tags (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE,
                IsPreDefined INTEGER NOT NULL,
                Color TEXT NOT NULL
            )";
            createTagsTable.ExecuteNonQuery();

            // Create EntryTags junction table
            var createEntryTagsTable = connection.CreateCommand();
            createEntryTagsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS EntryTags (
                EntryId INTEGER NOT NULL,
                TagId INTEGER NOT NULL,
                PRIMARY KEY (EntryId, TagId),
                FOREIGN KEY (EntryId) REFERENCES JournalEntries(Id) ON DELETE CASCADE,
                FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE
            )";
            createEntryTagsTable.ExecuteNonQuery();

            // Create Categories table
            var createCategoriesTable = connection.CreateCommand();
            createCategoriesTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Categories (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE,
                Icon TEXT NOT NULL,
                Color TEXT NOT NULL
            )";
            createCategoriesTable.ExecuteNonQuery();

            // Create Settings table
            var createSettingsTable = connection.CreateCommand();
            createSettingsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Settings (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL
            )";
            createSettingsTable.ExecuteNonQuery();

            // Seed default data
            SeedDefaultMoods(connection);
            SeedDefaultTags(connection);
            SeedDefaultCategories(connection);
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

        private void SeedDefaultTags(SqliteConnection connection)
        {
            var checkTags = connection.CreateCommand();
            checkTags.CommandText = "SELECT COUNT(*) FROM Tags";
            var count = Convert.ToInt32(checkTags.ExecuteScalar());

            if (count == 0)
            {
                var tags = Tag.GetDefaultTags();
                foreach (var tag in tags)
                {
                    var insert = connection.CreateCommand();
                    insert.CommandText = @"
                    INSERT INTO Tags (Name, IsPreDefined, Color)
                    VALUES (@Name, @IsPreDefined, @Color)";
                    insert.Parameters.AddWithValue("@Name", tag.Name);
                    insert.Parameters.AddWithValue("@IsPreDefined", tag.IsPreDefined ? 1 : 0);
                    insert.Parameters.AddWithValue("@Color", tag.Color);
                    insert.ExecuteNonQuery();
                }
            }
        }

        private void SeedDefaultCategories(SqliteConnection connection)
        {
            var checkCategories = connection.CreateCommand();
            checkCategories.CommandText = "SELECT COUNT(*) FROM Categories";
            var count = Convert.ToInt32(checkCategories.ExecuteScalar());

            if (count == 0)
            {
                var categories = Category.GetDefaultCategories();
                foreach (var category in categories)
                {
                    var insert = connection.CreateCommand();
                    insert.CommandText = @"
                    INSERT INTO Categories (Name, Icon, Color)
                    VALUES (@Name, @Icon, @Color)";
                    insert.Parameters.AddWithValue("@Name", category.Name);
                    insert.Parameters.AddWithValue("@Icon", category.Icon);
                    insert.Parameters.AddWithValue("@Color", category.Color);
                    insert.ExecuteNonQuery();
                }
            }
        }

        // Helper method to safely get column values
        private T GetValueOrDefault<T>(SqliteDataReader reader, int index, T defaultValue = default)
        {
            try
            {
                if (reader.IsDBNull(index))
                    return defaultValue;
                return (T)Convert.ChangeType(reader.GetValue(index), typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        // ==================== JOURNAL ENTRY CRUD ====================

        public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                    SELECT Id, Date, Title, Content, CreatedAt, UpdatedAt, 
                           PrimaryMoodId, SecondaryMood1Id, SecondaryMood2Id, CategoryId
                    FROM JournalEntries 
                    WHERE Date = @Date";
                    command.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));

                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        // FIXED: Proper nullable int handling
                        int? primaryMoodId = reader.IsDBNull(6) ? null : reader.GetInt32(6);
                        int? secondaryMood1Id = reader.IsDBNull(7) ? null : reader.GetInt32(7);
                        int? secondaryMood2Id = reader.IsDBNull(8) ? null : reader.GetInt32(8);
                        int? categoryId = reader.IsDBNull(9) ? null : reader.GetInt32(9);

                        System.Diagnostics.Debug.WriteLine($"=== DATABASE READ ===");
                        System.Diagnostics.Debug.WriteLine($"PrimaryMoodId from DB: {primaryMoodId}");
                        System.Diagnostics.Debug.WriteLine($"CategoryId from DB: {categoryId}");
                        System.Diagnostics.Debug.WriteLine("=====================");

                        return new JournalEntry
                        {
                            Id = reader.GetInt32(0),
                            Date = DateTime.Parse(reader.GetString(1)),
                            Title = reader.GetString(2),
                            Content = reader.GetString(3),
                            CreatedAt = DateTime.Parse(reader.GetString(4)),
                            UpdatedAt = DateTime.Parse(reader.GetString(5)),
                            PrimaryMoodId = primaryMoodId,
                            SecondaryMood1Id = secondaryMood1Id,
                            SecondaryMood2Id = secondaryMood2Id,
                            CategoryId = categoryId
                        };
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting entry by date: {ex.Message}");
                    return null;
                }
            });
        }

        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                    SELECT Id, Date, Title, Content, CreatedAt, UpdatedAt, 
                           PrimaryMoodId, SecondaryMood1Id, SecondaryMood2Id, CategoryId
                    FROM JournalEntries 
                    ORDER BY Date DESC";

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
                            PrimaryMoodId = GetValueOrDefault<int?>(reader, 6),
                            SecondaryMood1Id = GetValueOrDefault<int?>(reader, 7),
                            SecondaryMood2Id = GetValueOrDefault<int?>(reader, 8),
                            CategoryId = GetValueOrDefault<int?>(reader, 9)
                        });
                    }
                    return entries;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting all entries: {ex.Message}");
                    return new List<JournalEntry>();
                }
            });
        }

        public async Task<int> SaveEntryAsync(JournalEntry entry)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    var command = connection.CreateCommand();
                    if (entry.Id == 0)
                    {
                        // CREATE new entry
                        command.CommandText = @"
                        INSERT INTO JournalEntries 
                        (Date, Title, Content, CreatedAt, UpdatedAt, PrimaryMoodId, SecondaryMood1Id, SecondaryMood2Id, CategoryId)
                        VALUES (@Date, @Title, @Content, @CreatedAt, @UpdatedAt, @PrimaryMoodId, @SecondaryMood1Id, @SecondaryMood2Id, @CategoryId);
                        SELECT last_insert_rowid();";
                        command.Parameters.AddWithValue("@Date", entry.Date.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@Title", entry.Title);
                        command.Parameters.AddWithValue("@Content", entry.Content);
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("o"));
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("o"));
                        command.Parameters.AddWithValue("@PrimaryMoodId", (object?)entry.PrimaryMoodId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@SecondaryMood1Id", (object?)entry.SecondaryMood1Id ?? DBNull.Value);
                        command.Parameters.AddWithValue("@SecondaryMood2Id", (object?)entry.SecondaryMood2Id ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CategoryId", (object?)entry.CategoryId ?? DBNull.Value);

                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                    else
                    {
                        // UPDATE existing entry
                        command.CommandText = @"
                        UPDATE JournalEntries 
                        SET Title = @Title, Content = @Content, UpdatedAt = @UpdatedAt,
                            PrimaryMoodId = @PrimaryMoodId, SecondaryMood1Id = @SecondaryMood1Id, 
                            SecondaryMood2Id = @SecondaryMood2Id, CategoryId = @CategoryId
                        WHERE Id = @Id";
                        command.Parameters.AddWithValue("@Id", entry.Id);
                        command.Parameters.AddWithValue("@Title", entry.Title);
                        command.Parameters.AddWithValue("@Content", entry.Content);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("o"));
                        command.Parameters.AddWithValue("@PrimaryMoodId", (object?)entry.PrimaryMoodId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@SecondaryMood1Id", (object?)entry.SecondaryMood1Id ?? DBNull.Value);
                        command.Parameters.AddWithValue("@SecondaryMood2Id", (object?)entry.SecondaryMood2Id ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CategoryId", (object?)entry.CategoryId ?? DBNull.Value);

                        command.ExecuteNonQuery();
                        return entry.Id;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving entry: {ex.Message}");
                    return 0;
                }
            });
        }

        public async Task<bool> DeleteEntryAsync(int id)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM JournalEntries WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", id);

                    return command.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting entry: {ex.Message}");
                    return false;
                }
            });
        }

        // ==================== SEARCH & FILTER ====================

        public async Task<List<JournalEntry>> SearchEntriesAsync(
            string? searchText = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            List<int>? moodIds = null,
            List<int>? tagIds = null,
            int? categoryId = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    var query = new StringBuilder("SELECT DISTINCT e.Id, e.Date, e.Title, e.Content, e.CreatedAt, e.UpdatedAt, e.PrimaryMoodId, e.SecondaryMood1Id, e.SecondaryMood2Id, e.CategoryId FROM JournalEntries e ");
                    var conditions = new List<string>();
                    var command = connection.CreateCommand();

                    // Join with EntryTags if filtering by tags
                    if (tagIds != null && tagIds.Any())
                    {
                        query.Append("INNER JOIN EntryTags et ON e.Id = et.EntryId ");
                        conditions.Add($"et.TagId IN ({string.Join(",", tagIds)})");
                    }

                    // Search text in title or content
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        conditions.Add("(e.Title LIKE @SearchText OR e.Content LIKE @SearchText)");
                        command.Parameters.AddWithValue("@SearchText", $"%{searchText}%");
                    }

                    // Date range filter
                    if (startDate.HasValue)
                    {
                        conditions.Add("e.Date >= @StartDate");
                        command.Parameters.AddWithValue("@StartDate", startDate.Value.ToString("yyyy-MM-dd"));
                    }

                    if (endDate.HasValue)
                    {
                        conditions.Add("e.Date <= @EndDate");
                        command.Parameters.AddWithValue("@EndDate", endDate.Value.ToString("yyyy-MM-dd"));
                    }

                    // Mood filter
                    if (moodIds != null && moodIds.Any())
                    {
                        var moodConditions = new List<string>();
                        foreach (var moodId in moodIds)
                        {
                            moodConditions.Add($"e.PrimaryMoodId = {moodId} OR e.SecondaryMood1Id = {moodId} OR e.SecondaryMood2Id = {moodId}");
                        }
                        conditions.Add($"({string.Join(" OR ", moodConditions)})");
                    }

                    // Category filter
                    if (categoryId.HasValue)
                    {
                        conditions.Add("e.CategoryId = @CategoryId");
                        command.Parameters.AddWithValue("@CategoryId", categoryId.Value);
                    }

                    // Combine all conditions
                    if (conditions.Any())
                    {
                        query.Append("WHERE " + string.Join(" AND ", conditions));
                    }

                    query.Append(" ORDER BY e.Date DESC");

                    command.CommandText = query.ToString();

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
                            PrimaryMoodId = GetValueOrDefault<int?>(reader, 6),
                            SecondaryMood1Id = GetValueOrDefault<int?>(reader, 7),
                            SecondaryMood2Id = GetValueOrDefault<int?>(reader, 8),
                            CategoryId = GetValueOrDefault<int?>(reader, 9)
                        });
                    }
                    return entries;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error searching entries: {ex.Message}");
                    return new List<JournalEntry>();
                }
            });
        }

        // ==================== TAG OPERATIONS ====================

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Id, Name, IsPreDefined, Color FROM Tags ORDER BY Name";

                    var tags = new List<Tag>();
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        tags.Add(new Tag
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            IsPreDefined = reader.GetInt32(2) == 1,
                            Color = reader.GetString(3)
                        });
                    }
                    return tags;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting tags: {ex.Message}");
                    return new List<Tag>();
                }
            });
        }

        public async Task<int> CreateTagAsync(Tag tag)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                    INSERT INTO Tags (Name, IsPreDefined, Color)
                    VALUES (@Name, @IsPreDefined, @Color);
                    SELECT last_insert_rowid();";
                    command.Parameters.AddWithValue("@Name", tag.Name);
                    command.Parameters.AddWithValue("@IsPreDefined", tag.IsPreDefined ? 1 : 0);
                    command.Parameters.AddWithValue("@Color", tag.Color);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating tag: {ex.Message}");
                    return 0;
                }
            });
        }

        public async Task<List<Tag>> GetTagsForEntryAsync(int entryId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                    SELECT t.Id, t.Name, t.IsPreDefined, t.Color
                    FROM Tags t
                    INNER JOIN EntryTags et ON t.Id = et.TagId
                    WHERE et.EntryId = @EntryId
                    ORDER BY t.Name";
                    command.Parameters.AddWithValue("@EntryId", entryId);

                    var tags = new List<Tag>();
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        tags.Add(new Tag
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            IsPreDefined = reader.GetInt32(2) == 1,
                            Color = reader.GetString(3)
                        });
                    }
                    return tags;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting tags for entry: {ex.Message}");
                    return new List<Tag>();
                }
            });
        }

        public async Task SaveEntryTagsAsync(int entryId, List<int> tagIds)
        {
            await Task.Run(() =>
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    // Delete existing tags for this entry
                    var deleteCommand = connection.CreateCommand();
                    deleteCommand.CommandText = "DELETE FROM EntryTags WHERE EntryId = @EntryId";
                    deleteCommand.Parameters.AddWithValue("@EntryId", entryId);
                    deleteCommand.ExecuteNonQuery();

                    // Insert new tags
                    foreach (var tagId in tagIds)
                    {
                        var insertCommand = connection.CreateCommand();
                        insertCommand.CommandText = @"
                        INSERT INTO EntryTags (EntryId, TagId)
                        VALUES (@EntryId, @TagId)";
                        insertCommand.Parameters.AddWithValue("@EntryId", entryId);
                        insertCommand.Parameters.AddWithValue("@TagId", tagId);
                        insertCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving entry tags: {ex.Message}");
                }
            });
        }

        // ==================== CATEGORY OPERATIONS ====================

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Id, Name, Icon, Color FROM Categories ORDER BY Name";

                    var categories = new List<Category>();
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        categories.Add(new Category
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Icon = reader.GetString(2),
                            Color = reader.GetString(3)
                        });
                    }
                    return categories;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting categories: {ex.Message}");
                    return new List<Category>();
                }
            });
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Id, Name, Icon, Color FROM Categories WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", id);

                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        return new Category
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Icon = reader.GetString(2),
                            Color = reader.GetString(3)
                        };
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting category: {ex.Message}");
                    return null;
                }
            });
        }

        // ==================== MOOD OPERATIONS ====================

        public async Task<List<Mood>> GetAllMoodsAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Id, Name, Category, Emoji FROM Moods ORDER BY Category, Name";

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
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting moods: {ex.Message}");
                    return new List<Mood>();
                }
            });
        }

        public async Task<Mood?> GetMoodByIdAsync(int id)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Id, Name, Category, Emoji FROM Moods WHERE Id = @Id";
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
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting mood: {ex.Message}");
                    return null;
                }
            });
        }

        // ==================== SETTINGS (THEME) ====================

        public async Task<string?> GetSettingAsync(string key)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Value FROM Settings WHERE Key = @Key";
                    command.Parameters.AddWithValue("@Key", key);

                    var result = command.ExecuteScalar();
                    return result?.ToString();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting setting: {ex.Message}");
                    return null;
                }
            });
        }

        public async Task SaveSettingAsync(string key, string value)
        {
            await Task.Run(() =>
            {
                try
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
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving setting: {ex.Message}");
                }
            });
        }
    }
}