using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;

namespace XamlEditor
{
    class XmlEditor
    {
        private RichTextBox rtbEditor;

        public bool AutoUpdate { get; set; }

        public int TimerIntervalInSeconds;

        private DispatcherTimer dispatcherTimer = new DispatcherTimer();

        public XmlEditor(RichTextBox rtb, bool autoUpdate = false, int intervalInSeconds = 3) {
            rtbEditor = rtb;
            rtbEditor.TextChanged += rtbEditor_TextChanged;
            AutoUpdate = autoUpdate;
            TimerIntervalInSeconds = intervalInSeconds;
        }

        public void Refresh(bool messageBoxWarning = false)
        {
            string textBeforeCursor = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.CaretPosition).Text;
            TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
            string TextString = range.Text;
            try
            {
                XmlReader nodeReader = XmlReader.Create(new StringReader(TextString));
                nodeReader.MoveToContent();
                XmlDocument doc = new XmlDocument();
                doc.Load(nodeReader);
                rtbEditor.Document.Blocks.Clear();
                FlowDocument rtb = rtbEditor.Document;
                Paragraph p = new Paragraph();
                OutputText(doc.DocumentElement, 0, p);
                rtb.Blocks.Add(p);
            }
            catch
            {
                if (messageBoxWarning)
                {
                    MessageBox.Show("The file does not match the language syntax.");
                }
            }
            
            GetOffsetOfCursor(textBeforeCursor);
        }

        public void OpenFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                rtbEditor.Document.Blocks.Clear();
                FileStream fileStream = new FileStream(dlg.FileName, FileMode.Open);
                TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                range.Load(fileStream, DataFormats.Text);
            }
        }

        public void SaveFile()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(dlg.FileName, FileMode.Create);
                TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                range.Save(fileStream, DataFormats.Text);
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Refresh();
            dispatcherTimer.Stop();
        }

        private void rtbEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!dispatcherTimer.IsEnabled && AutoUpdate)
            {
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, TimerIntervalInSeconds);
                dispatcherTimer.Start();
            }
        }

        private void Tabs(int num, Paragraph p)
        {
            string tabs = "";
            Span span = new Span() { Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0)) };
            for (int j = 0; j < num; j++)
                tabs += "\t";
            span.Inlines.Add(tabs);
            p.Inlines.Add(span);
        }

        private void AddSpan(string text, SolidColorBrush color, Paragraph p)
        {
            Span span = new Span() { Foreground = color };
            span.Inlines.Add(text);
            p.Inlines.Add(span);
        }

        private void OutputText(XmlNode xnode, int num, Paragraph p)
        {
            if (xnode.HasChildNodes)
            {
                Tabs(num, p);
                AddSpan("<", TextColors.Blue, p);
                AddSpan($"{xnode.Name}", TextColors.Brown, p);
                foreach (XmlAttribute atr in xnode.Attributes)
                {
                    AddSpan($" {atr.Name}", TextColors.Red, p);
                    AddSpan($"=\"{atr.Value}\"", TextColors.Blue, p);
                }
                AddSpan(">\n", TextColors.Blue, p);
                foreach (XmlNode e in xnode.ChildNodes)
                {
                    OutputText(e, num + 1, p);
                }
                Tabs(num, p);
                AddSpan("</", TextColors.Blue, p);
                AddSpan($"{xnode.Name}", TextColors.Brown, p);
                AddSpan(">\n", TextColors.Blue, p);
            }
            else
            {
                if (xnode.Attributes != null)
                {
                    Tabs(num, p);
                    AddSpan("<", TextColors.Blue, p);
                    AddSpan($"{xnode.Name}", TextColors.Brown, p);
                    foreach (XmlAttribute atr in xnode.Attributes)
                    {
                        AddSpan($" {atr.Name}", TextColors.Red, p);
                        AddSpan($"=\"{atr.Value}\"", TextColors.Blue, p);
                    }
                    AddSpan(">", TextColors.Blue, p);
                    AddSpan("</", TextColors.Blue, p);
                    AddSpan($"{xnode.Name}", TextColors.Brown, p);
                    AddSpan(">\n", TextColors.Blue, p);
                }
                else
                {
                    Tabs(num, p);
                    string value = xnode.Value.Replace(Environment.NewLine, "");
                    value = value.Replace("\n", "");
                    value = value.Replace("\t", "");
                    AddSpan($"{value}\n", TextColors.Black, p);
                }

            }
        }

        private void GetOffsetOfCursor(string textBeforeCursor)
        {
            TextPointer cur = rtbEditor.Document.ContentStart;
            string text = new TextRange(rtbEditor.Document.ContentStart, cur).Text;
            string needtext = textBeforeCursor.Replace(" ", "");
            needtext = needtext.Replace(" ", "");
            needtext = needtext.Replace("\n", "");
            needtext = needtext.Replace("\r", "");
            needtext = needtext.Replace("\t", "");
            needtext = needtext.Replace(Environment.NewLine, "");
            while (text != needtext && cur.GetOffsetToPosition(rtbEditor.Document.ContentEnd) > 0)
            {
                cur = cur.GetPositionAtOffset(1);
                text = new TextRange(rtbEditor.Document.ContentStart, cur).Text;
                text = text.Replace(" ", "");
                text = text.Replace("\n", "");
                text = text.Replace("\t", "");
                text = text.Replace("\r", "");
                text = text.Replace(Environment.NewLine, "");
            }
            rtbEditor.CaretPosition = cur;
        }
    }
}
