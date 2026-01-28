using PersonalJournalDesktopApp.ViewModels;

namespace PersonalJournalDesktopApp.Views
{
    [QueryProperty(nameof(Date), "Date")]
    public partial class EntryDetailPage : ContentPage
    {
        private readonly EntryDetailViewModel _viewModel;
        private DateTime _date;
        private bool _isEditorLoaded = false;

        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                if (_viewModel != null)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await _viewModel.InitializeAsync(value);
                        await LoadEditorAsync();
                    });
                }
            }
        }

        public EntryDetailPage(EntryDetailViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Set up WebView
            EditorWebView.Navigated += OnEditorNavigated;

            // FIXED: Subscribe to save event to get content BEFORE saving
            _viewModel.SaveRequested += OnSaveRequested;
        }

        private async void OnSaveRequested(object? sender, EventArgs e)
        {
            // Get content from editor and set it in ViewModel BEFORE saving
            if (_isEditorLoaded)
            {
                _viewModel.Content = await GetEditorContentAsync();
            }
        }

        private async Task LoadEditorAsync()
        {
            try
            {
                // Load the Quill editor HTML
                var htmlContent = await LoadQuillHtmlAsync();
                var htmlSource = new HtmlWebViewSource { Html = htmlContent };
                EditorWebView.Source = htmlSource;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading editor: {ex.Message}");
                await DisplayAlert("Error", "Could not load rich text editor", "OK");
            }
        }

        private async Task<string> LoadQuillHtmlAsync()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("quill-editor.html");
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
            catch
            {
                // Fallback: return embedded HTML if file not found
                return GetEmbeddedQuillHtml();
            }
        }

        private void OnEditorNavigated(object sender, WebNavigatedEventArgs e)
        {
            if (e.Result == WebNavigationResult.Success)
            {
                _isEditorLoaded = true;

                // Load existing content if any
                if (!string.IsNullOrEmpty(_viewModel.Content))
                {
                    _ = SetEditorContentAsync(_viewModel.Content);
                }
            }
        }

        private async Task<string> GetEditorContentAsync()
        {
            if (!_isEditorLoaded) return string.Empty;

            try
            {
                var result = await EditorWebView.EvaluateJavaScriptAsync("window.quillEditor.getContent()");
                return result ?? string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting content: {ex.Message}");
                return string.Empty;
            }
        }

        private async Task SetEditorContentAsync(string html)
        {
            if (!_isEditorLoaded) return;

            try
            {
                // Escape the HTML for JavaScript
                var escapedHtml = html.Replace("\\", "\\\\")
                                     .Replace("`", "\\`")
                                     .Replace("$", "\\$")
                                     .Replace("\r", "\\r")
                                     .Replace("\n", "\\n");

                await EditorWebView.EvaluateJavaScriptAsync($"window.quillEditor.setContent(`{escapedHtml}`)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting content: {ex.Message}");
            }
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            // Unsubscribe from event
            _viewModel.SaveRequested -= OnSaveRequested;

            // Also save content when leaving (backup)
            if (_isEditorLoaded)
            {
                _viewModel.Content = await GetEditorContentAsync();
            }
        }

        // Embedded HTML if file is not found
        private string GetEmbeddedQuillHtml()
        {
            return @"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <link href='https://cdn.quilljs.com/1.3.6/quill.snow.css' rel='stylesheet'>
    <style>
        body { margin: 0; padding: 0; font-family: system-ui; }
        #editor-container { height: 100vh; }
        .ql-toolbar { background-color: #f8f9fa; border: none !important; border-bottom: 1px solid #e5e7eb !important; }
        .ql-container { border: none !important; font-size: 16px; }
        .ql-editor { padding: 20px; line-height: 1.6; }
    </style>
</head>
<body>
    <div id='editor-container'></div>
    <script src='https://cdn.quilljs.com/1.3.6/quill.js'></script>
    <script>
        var quill = new Quill('#editor-container', {
            theme: 'snow',
            placeholder: 'Write your journal entry here...',
            modules: {
                toolbar: [
                    ['bold', 'italic', 'underline', 'strike'],
                    [{ 'header': [1, 2, 3, false] }],
                    [{ 'list': 'ordered'}, { 'list': 'bullet' }],
                    [{ 'indent': '-1'}, { 'indent': '+1' }],
                    [{ 'color': [] }, { 'background': [] }],
                    [{ 'align': [] }],
                    ['blockquote', 'code-block'],
                    ['clean']
                ]
            }
        });
        window.quillEditor = {
            getContent: function() { return quill.root.innerHTML; },
            setContent: function(html) { quill.root.innerHTML = html; },
            getPlainText: function() { return quill.getText(); }
        };
    </script>
</body>
</html>";
        }
    }
}