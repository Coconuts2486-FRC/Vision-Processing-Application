using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VisionProcessing2._0
{
    /// <summary>
    /// Interaction logic for ChangeParameters.xaml
    /// </summary>
    public partial class ChangeParameters : Window
    {
        public ChangeParameters()
        {
            InitializeComponent();
        }

        private void Finish(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(lBri.Text, out StaticResources.minBrightness)) MessageBox.Show("Lower brightness is not a valid number.");
            else if (!double.TryParse(uBri.Text, out StaticResources.maxBrightness)) MessageBox.Show("Upper brightness is not a valid number.");
            else if (!double.TryParse(lExp.Text, out StaticResources.minExposure)) MessageBox.Show("Lower exposure is not a valid number.");
            else if (!double.TryParse(uExp.Text, out StaticResources.maxExposure)) MessageBox.Show("Upper exposure is not a valid number.");
            else if (!double.TryParse(lFoc.Text, out StaticResources.minFocus)) MessageBox.Show("Lower focus is not a valid number.");
            else if (!double.TryParse(uFoc.Text, out StaticResources.maxFocus)) MessageBox.Show("Upper focus is not a valid number.");
            else
            {
                this.Close();
            }
        }
    }
}
