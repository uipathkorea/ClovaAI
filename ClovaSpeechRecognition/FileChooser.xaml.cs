using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace ClovaSpeech
{
    // FileChooser.xaml에 대한 상호 작용 논리
    public partial class FileChooser
    {
        private OpenFileDialog openFileDialog1;
        public FileChooser()
        {
            InitializeComponent();
        }
        private void FileChooser_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();
        }
    }
}
