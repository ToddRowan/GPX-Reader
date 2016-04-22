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
using kimandtodd.DG200CSharp.commandresults.resultitems;

namespace kimandtodd.GPX_Reader
{
    /// <summary>
    /// Interaction logic for ConfigDialog.xaml
    /// </summary>
    public partial class ConfigDialog : Window
    {
        private DG200Configuration _cfg;
        private DG200SerialConnection _sc;

        public ConfigDialog()
        {
            InitializeComponent();
        }

        public void setSerialConnection(DG200SerialConnection sc)
        {
            this._sc = sc;
        }

        public void populate(DG200Configuration cfg)
        {
            this.chkWaas.IsChecked = cfg.getEnableWaas();
            this.chkAltitude.IsChecked = (cfg.getTrackingType() == BaseDGTrackPoint.FORMAT_POSITION_DATE_SPEED_ALTITUDE);

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

            this.txtSecondsIntvl.Text = (cfg.getTimeInterval()/1000).ToString("D");
            this.txtMetersIntvl.Text = cfg.getDistanceInterval().ToString();

            this.chkDistance.IsChecked = cfg.getUseDistanceThreshold();
            this.chkSpeed.IsChecked = cfg.getUseSpeedThreshold();

            this.txtMinSpeed.Text = cfg.getSpeedThresholdValue().ToString();
            this.txtMinDistance.Text = cfg.getDistanceThresholdValue().ToString();

            this._cfg = cfg;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Reads the config data from the UI.
        /// </summary>
        /// <returns>The updated configuration values. </returns>
        public DG200Configuration generateConfig()
        {
            this._cfg.setEnableWaas(this.chkWaas.IsChecked == true);

            this._cfg.setTrackingType(this.chkAltitude.IsChecked == true ? BaseDGTrackPoint.FORMAT_POSITION_DATE_SPEED_ALTITUDE : BaseDGTrackPoint.FORMAT_POSITION_DATE_SPEED);

            this._cfg.setUseTimeInterval(this.rdoUseTime.IsChecked == true);

            this._cfg.setTimeInterval(UInt32.Parse(this.txtSecondsIntvl.Text)*1000);
            this._cfg.setDistanceInterval(UInt32.Parse(this.txtMetersIntvl.Text));

            this._cfg.setUseDistanceThreshold(this.chkDistance.IsChecked == true);
            this._cfg.setUseSpeedThreshold(this.chkSpeed.IsChecked == true);

            this._cfg.setSpeedThresholdValue(UInt32.Parse(this.txtMinSpeed.Text));
            this._cfg.setDistanceThresholdValue(UInt32.Parse(this.txtMinDistance.Text));

            return this._cfg;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DG200Configuration cfg = this.generateConfig();

            SetDGConfigurationCommand set = new SetDGConfigurationCommand(cfg);
            set.setSerialConnection(this._sc);

            try
            {
                set.execute();
            }
            catch (kimandtodd.DG200CSharp.commands.exceptions.CommandException ex)
            {
                Console.WriteLine("An exception was thrown when trying to set the configuration: " + ex.Message);
            }

            SetDGConfigurationCommandResult setR = (SetDGConfigurationCommandResult)set.getLastResult();
            kimandtodd.GPX_Reader.MainWindow win = this.Owner as kimandtodd.GPX_Reader.MainWindow;

            win.lblStatus.Content = "Connected on port " + this._sc.getPortName() + ".\n";
            win.lblStatus.Content += setR.getSuccess() ? "Configuration saved." : "Configuration not saved!";

            this.Close();
        }
    }
}
