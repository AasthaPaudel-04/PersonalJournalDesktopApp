using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PersonalJournalDesktopApp.Models;

namespace PersonalJournalDesktopApp.Services
{
    public class ExportService
    {
        public async Task<string> ExportToPdfAsync(List<JournalEntry> entries, string fileName)
        {
            // Create HTML content
            var html = GenerateHtmlContent(entries);

            // Save as HTML file (can be printed to PDF by user)
            var filePath = Path.Combine(FileSystem.AppDataDirectory, $"{fileName}.html");
            await File.WriteAllTextAsync(filePath, html);

            return filePath;
        }

        private string GenerateHtmlContent(List<JournalEntry> entries)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='utf-8'>");
            sb.AppendLine("<style>");
            sb.AppendLine(@"
                body {
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    max-width: 800px;
                    margin: 40px auto;
                    padding: 20px;
                    background-color: #ffffff;
                    color: #1f2937; /* dark blue-gray text */
                }

                .header {
                    text-align: center;
                    margin-bottom: 40px;
                    border-bottom: 3px solid #3b82f6; /* blue */
                    padding-bottom: 20px;
                }

                .header h1 {
                    color: #1d4ed8; /* deep blue */
                }

                .entry {
                    background-color: #f8fafc; /* very light blue/gray */
                    padding: 25px;
                    margin-bottom: 30px;
                    border-radius: 12px;
                    border: 1px solid #e5e7eb;
                    page-break-inside: avoid;
                }

                .entry-date {
                    color: #64748b; /* muted blue-gray */
                    font-size: 14px;
                    margin-bottom: 8px;
                }

                .entry-title {
                    font-size: 24px;
                    font-weight: bold;
                    margin-bottom: 15px;
                    color: #1e40af; /* strong blue */
                }

                .entry-content {
                    line-height: 1.8;
                    white-space: pre-wrap;
                    margin-bottom: 15px;
                    color: #1f2937;
                }

                .entry-moods {
                    display: flex;
                    gap: 10px;
                    margin-top: 15px;
                    flex-wrap: wrap;
                }

                .mood-badge {
                    background-color: #3b82f6; /* blue */
                    color: white;
                    padding: 6px 14px;
                    border-radius: 16px;
                    font-size: 12px;
                }

                .tags {
                    display: flex;
                    gap: 8px;
                    flex-wrap: wrap;
                    margin-top: 10px;
                }

                .tag {
                    background-color: #e0f2fe; /* light blue */
                    color: #0369a1;
                    padding: 5px 12px;
                    border-radius: 14px;
                    font-size: 11px;
                    border: 1px solid #bae6fd;
                }

                .category {
                    display: inline-block;
                    background-color: #1d4ed8;
                    color: white;
                    padding: 6px 14px;
                    border-radius: 16px;
                    font-size: 12px;
                    margin-top: 12px;
                }

                @media print {
                    body {
                        background-color: white;
                    }

                    .entry {
                        border: 1px solid #cbd5e1;
                    }
                }
            ");

            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Header
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<h1>My Journal</h1>");
            sb.AppendLine($"<p>Exported on {DateTime.Now:MMMM dd, yyyy}</p>");
            sb.AppendLine($"<p>Total Entries: {entries.Count}</p>");
            sb.AppendLine("</div>");

            // Entries
            foreach (var entry in entries.OrderByDescending(e => e.Date))
            {
                sb.AppendLine("<div class='entry'>");
                sb.AppendLine($"<div class='entry-date'>{entry.Date:dddd, MMMM dd, yyyy}</div>");
                sb.AppendLine($"<div class='entry-title'>{System.Net.WebUtility.HtmlEncode(entry.Title)}</div>");
                sb.AppendLine($"<div class='entry-content'>{System.Net.WebUtility.HtmlEncode(entry.Content)}</div>");

                // Moods
                var moods = new List<string>();
                if (entry.PrimaryMood != null)
                    moods.Add($"{entry.PrimaryMood.Emoji} {entry.PrimaryMood.Name}");
                if (entry.SecondaryMood1 != null)
                    moods.Add($"{entry.SecondaryMood1.Emoji} {entry.SecondaryMood1.Name}");
                if (entry.SecondaryMood2 != null)
                    moods.Add($"{entry.SecondaryMood2.Emoji} {entry.SecondaryMood2.Name}");

                if (moods.Any())
                {
                    sb.AppendLine("<div class='entry-moods'>");
                    foreach (var mood in moods)
                    {
                        sb.AppendLine($"<span class='mood-badge'>{mood}</span>");
                    }
                    sb.AppendLine("</div>");
                }

                // Category
                if (entry.Category != null)
                {
                    sb.AppendLine($"<div class='category'>{entry.Category.Icon} {entry.Category.Name}</div>");
                }

                // Tags
                if (entry.Tags.Any())
                {
                    sb.AppendLine("<div class='tags'>");
                    foreach (var tag in entry.Tags)
                    {
                        sb.AppendLine($"<span class='tag'>{System.Net.WebUtility.HtmlEncode(tag.Name)}</span>");
                    }
                    sb.AppendLine("</div>");
                }

                sb.AppendLine("</div>");
            }

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}