using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using kimandtodd.DG200CSharp;
using kimandtodd.DG200CSharp.commands;
using kimandtodd.DG200CSharp.commandresults;
using kimandtodd.DG200CSharp.commandresults.resultitems;


namespace kimandtodd.GPX_Reader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PortScanner _ports;
        private DG200Configuration _dgConfig;
        private DG200SerialConnection _dgSerialConnection;
        private ConfigDialog _cd;

        private TrackHeaderEntry _currentTrackHeaderEntry;

        // See http://stackoverflow.com/questions/8653897/how-to-addcombobox-items-dynamically-in-wpf 
        public ObservableCollection<ComboBoxItem> cbItems { get; set; }
        public ObservableCollection<ListViewItem> TrackHeaders { get; set; }
        public ComboBoxItem SelectedcbItem { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            this._ports = new PortScanner();

            initializeObservers();

            populatePortList();
        }

        private DG200SerialConnection getSerialConnection()
        {
            if (this._dgSerialConnection == null)
            {
                ComboBoxItem selected = this.cbPorts.SelectedItem as ComboBoxItem;
                this._dgSerialConnection = new DG200SerialConnection(selected.Content as string);
            }

            return this._dgSerialConnection;            
        }

        private void MenuEdit_Click(object sender, RoutedEventArgs e)
        {
            this._cd = new ConfigDialog();
            _cd.ShowInTaskbar = false;
            _cd.Owner = Application.Current.MainWindow;
            this.populateConfigDialog();
            _cd.ShowDialog();
        }

        private void populateConfigDialog()
        {
            _cd.populate(this._dgConfig);
        }

        private void MenuPorts_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuErase_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                DeleteAllDGTrackFilesCommand c = new DeleteAllDGTrackFilesCommand();
                c.setSerialConnection(this.getSerialConnection());

                c.execute();

                DeleteAllTrackFilesCommandResult cr = (DeleteAllTrackFilesCommandResult)c.getLastResult();
                
                this.lblStatus.Content = "Connected on port " + this.getSerialConnection().getPortName() + ".\n";
                this.lblStatus.Content += cr.getSuccess() ? "Track files deleted.\n0% memory consumed." : "Error deleting track files.";
            }
            catch (kimandtodd.DG200CSharp.commands.exceptions.CommandException ex)
            {
                this.lblStatus.Content = "Deletion failed: " + ex.Message;
            }
        }

        private void AppExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void refreshTrackHeaders_Click(object sender, RoutedEventArgs e)
        {
            this.refreshTrackHeaders();
        }

        private void HandleTrackHeaderSelection(object sender, SelectionChangedEventArgs args)
        {
            ListView lv = sender as ListView;

            if (lv.SelectedItems.Count > 0)
            {
                this.btnDownload.IsEnabled = true;
            }
            else
            {
                this.btnDownload.IsEnabled = false;
            }
        }

        private void HandlePortNameSelection(object sender, SelectionChangedEventArgs args)
        {
            ComboBox cb = sender as ComboBox;

            if (cb.SelectedIndex != 0)
            {
                this.btnConnect.IsEnabled = true;
            }
            else
            {
                this.btnConnect.IsEnabled = false;
            }
        }

        private void populatePortList()
        {
            var cbItem = new ComboBoxItem { Content = "Not selected" };
            SelectedcbItem = cbItem;
            cbItems.Add(cbItem);            

            foreach (string p in this._ports.getPorts())
            {                
                cbItems.Add(new ComboBoxItem { Content = p });
            }
        }

        private void initializeObservers()
        {
            cbItems = new ObservableCollection<ComboBoxItem>();
            TrackHeaders = new ObservableCollection<ListViewItem>();
        }

        private void refreshTrackHeaders()
        {
            try
            {
                GetDGTrackHeadersCommand c = new GetDGTrackHeadersCommand();
                c.setSerialConnection(this.getSerialConnection());

                c.execute();

                GetDGTrackHeadersCommandResult cr = (GetDGTrackHeadersCommandResult)c.getLastResult();

                foreach (DGTrackHeader th in cr.getTrackHeaders())
                {
                    if (!th.getIsValid())
                    {
                        continue;
                    }
                        
                    if (th.getIsFirstBlock())
                    {
                        this.addCurrentTrackHeaderEntry();

                        this._currentTrackHeaderEntry = new TrackHeaderEntry(th);
                    }
                    else 
                    {
                        this._currentTrackHeaderEntry.addTrackId(th.getFileIndex());
                    }
                }

                this.addCurrentTrackHeaderEntry();

                this.lblStatus.Content = "Connected on port " + this.getSerialConnection().getPortName() + ".\n";
                this.lblStatus.Content += this._dgConfig.getMemoryUsage() + "% memory consumed.\n";
                this.lblStatus.Content += this.TrackHeaders.Count + " track headers retrieved.";
            }
            catch (kimandtodd.DG200CSharp.commands.exceptions.CommandException ex)
            {
                this.lblStatus.Content += "Retrieving track headers failed: " + ex.Message;
            }
        }

        private void addCurrentTrackHeaderEntry()
        {
            if (this._currentTrackHeaderEntry != null)
            {
                this.TrackHeaders.Add(new ListViewItem { Content = this._currentTrackHeaderEntry });
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                GetDGConfigurationCommand c = new GetDGConfigurationCommand();
                c.setSerialConnection(this.getSerialConnection());

                c.execute();

                GetDGConfigurationCommandResult cr = (GetDGConfigurationCommandResult)c.getLastResult();
                this._dgConfig = cr.getConfiguration();

                this.refreshTrackHeaders();
            }
            catch (kimandtodd.DG200CSharp.commands.exceptions.CommandException ex)
            {
                this.lblStatus.Content = "Connection failed: " + ex.Message;
            }
        }
    }
}
