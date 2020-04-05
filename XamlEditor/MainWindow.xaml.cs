using System.Windows;

namespace XamlEditor
{
	public partial class MainWindow : Window
    {

        private XmlEditor xmlEditor;

        public MainWindow()
        {
            InitializeComponent();
            xmlEditor = new XmlEditor(rtbEditor);
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            xmlEditor.SaveFile();
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            xmlEditor.OpenFile();
        }

        private void Format(object sender, RoutedEventArgs e)
        {
            xmlEditor.Refresh(true);
        }

        private void AutoFormatting_Checked(object sender, RoutedEventArgs e)
        {
            xmlEditor.AutoUpdate = true;
            xmlEditor.Refresh();
        }

        private void AutoFormatting_Unchecked(object sender, RoutedEventArgs e)
        {
            xmlEditor.AutoUpdate = false;
        }

        private void NewWindow(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
        }
    }
}
