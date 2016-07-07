using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace VisionProcessing2._0
{
    class TextBoxOutputter : TextWriter
    {
        TextBox textBox = null;
        public TextBoxOutputter(TextBox output)
        {
            textBox = output;
        }

        public override void Write(char value)
        {
            base.Write(value);
            textBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                textBox.AppendText(value.ToString());
            }));
            textBox.Focus();
            textBox.CaretIndex = textBox.Text.Length;
            textBox.ScrollToEnd();
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
