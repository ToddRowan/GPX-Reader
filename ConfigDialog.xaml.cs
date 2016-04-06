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

using kimandtodd.DG200CSharp;
using kimandtodd.DG200CSharp.commands;
using kimandtodd.DG200CSharp.commandresults;

namespace kimandtodd.GPX_Reader
{
    /// <summary>
    /// Interaction logic for ConfigDialog.xaml
    /// </summary>
    public partial class ConfigDialog : Window
    {
        public ConfigDialog()
        {
            InitializeComponent();
        }

        public void populate(DG200Configuration cfg)
        {
            this.chkWaas.IsChecked = cfg.getEnableWaas();
            this.chkAltitude.IsChecked = (cfg.getTrackingType() == 3);

            if (cfg.getUseTimeInterval())
            {
                this.rdoUseTime.IsChecked = true;
                this.rdoUseDist.IsChecked = false;
            }
            else
            {
                this.rdoUseTime.IsChecked = false;
                this.rdoUseDist.IsChecked = true;
            }

            this.txtSecondsIntvl.Text = cfg.getTimeInterval().ToString();
            this.txtMetersIntvl.Text = cfg.getDistanceInterval().ToString();

            this.chkDistance.IsChecked = cfg.getUseDistanceThreshold();
            this.chkSpeed.IsChecked = cfg.getUseSpeedThreshold();

            this.txtMinSpeed.Text = cfg.getSpeedThresholdValue().ToString();
            this.txtMinDistance.Text = cfg.getDistanceThresholdValue().ToString();
        }
    }
}
