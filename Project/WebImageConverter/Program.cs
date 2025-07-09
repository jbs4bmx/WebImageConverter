using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using System.Reflection;

namespace WebImageConverter
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ConverterForm());
        }
    }

    public class ConverterForm : Form
    {
        private Button browseButton;
        private TextBox folderPathBox;
        private ComboBox formatBox;
        private Button convertButton;
        private TextBox logBox;
        private CheckBox recursiveCheckBox;
        private FolderBrowserDialog folderDialog;
        private Label countLabel;
        private ProgressBar progressBar;

        public ConverterForm()
        {
            var version = Application.ProductVersion;
            var assembly = Assembly.GetExecutingAssembly();
            using Stream? iconStream = assembly.GetManifestResourceStream("WebImageConverter.wici.ico");
            if (iconStream != null)
            {
                this.Icon = new Icon(iconStream);
            }
            else
            {
                MessageBox.Show("Embedded icon not found.");
            }
            Text = $"WebP Image Converter - v{version}";
            Width = 600;
            Height = 400;

            folderPathBox = new TextBox { Left = 20, Top = 20, Width = 400 };
            browseButton = new Button { Text = "Browse...", Left = 430, Top = 18, Width = 100 };
            formatBox = new ComboBox { Left = 20, Top = 60, Width = 100 };
            convertButton = new Button { Text = "Convert", Left = 130, Top = 58, Width = 100 };
            logBox = new TextBox { Left = 20, Top = 100, Width = 540, Height = 220, Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true };

            recursiveCheckBox = new CheckBox
            {
                Text = "Include subfolders",
                Left = 250,
                Top = 62,
                Width = 150,
                Checked = true
            };

            countLabel = new Label
            {
                Text = "Files found: 0",
                Left = 20,
                Top = 330,
                Width = 200
            };

            progressBar = new ProgressBar
            {
                Left = 20,
                Top = 360,
                Width = 540,
                Height = 20,
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            formatBox.Items.AddRange(new string[] { "png", "jpg" });
            formatBox.SelectedIndex = 0;

            browseButton.Click += BrowseButton_Click;
            convertButton.Click += ConvertButton_Click;

            Controls.Add(folderPathBox);
            Controls.Add(browseButton);
            Controls.Add(formatBox);
            Controls.Add(convertButton);
            Controls.Add(logBox);
            Controls.Add(recursiveCheckBox);
            Controls.Add(countLabel);
            Controls.Add(progressBar);

            folderDialog = new FolderBrowserDialog();
        }

        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                folderPathBox.Text = folderDialog.SelectedPath;
            }
        }

        private void ConvertButton_Click(object? sender, EventArgs e)
        {
            string folderPath = folderPathBox.Text;
            string format = formatBox.SelectedItem?.ToString()?.ToLower() ?? "png";

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("Please select a valid folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            IImageEncoder encoder = format == "png"
                ? new PngEncoder()
                : new JpegEncoder { Quality = 90 };

            var searchOption = recursiveCheckBox.Checked
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            var files = Directory.GetFiles(folderPath, "*.webp", searchOption);
            int total = files.Length;

            countLabel.Text = $"Files found: {total}";
            progressBar.Value = 0;
            progressBar.Maximum = total > 0 ? total : 1;

            logBox.Clear();
            logBox.AppendText($"Found {total} .webp files.\r\n");

            int converted = 0;

            foreach (var file in files)
            {
                try
                {
                    using var image = SixLabors.ImageSharp.Image.Load(file);
                    string newFile = Path.ChangeExtension(file, format);
                    image.Save(newFile, encoder);
                    logBox.AppendText($"✔ Converted: {Path.GetFileName(file)}\r\n");
                }
                catch (Exception ex)
                {
                    logBox.AppendText($"✖ Failed: {Path.GetFileName(file)} - {ex.Message}\r\n");
                }

                converted++;
                progressBar.Value = converted;
                progressBar.Refresh();
            }

            logBox.AppendText("✅ Conversion complete.\r\n");
        }
    }
}
