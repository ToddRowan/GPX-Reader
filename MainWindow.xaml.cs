using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

using kimandtodd.DG200CSharp;
using kimandtodd.DG200CSharp.commands;
using kimandtodd.DG200CSharp.commandresults;
using kimandtodd.DG200CSharp.commandresults.resultitems;
using kimandtodd.DG200CSharp.logging;


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
        private ProgressDialog _pd;

        private TrackHeaderEntry _currentTrackHeaderEntry;

        // See http://stackoverflow.com/questions/8653897/how-to-addcombobox-items-dynamically-in-wpf 
        public ObservableCollection<ComboBoxItem> cbItems { get; set; }
        public ObservableCollection<System.Windows.Controls.ListViewItem> TrackHeaders { get; set; }
        public ComboBoxItem SelectedcbItem { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            DG200FileLogger.setLevel(3);

            this._ports = new PortScanner();

            this.initializeObservers();

            this.populatePortList();
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
            this._cd.setSerialConnection(this._dgSerialConnection);
            _cd.ShowInTaskbar = false;
            _cd.Owner = System.Windows.Application.Current.MainWindow;
            this.populateConfigDialog();
            _cd.ShowDialog();
        }

        private void populateConfigDialog()
        {
            _cd.populate(this._dgConfig);
        }

        private void MenuPorts_Click(object sender, RoutedEventArgs e)
        {
            this.populatePortList();
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuErase_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                // TODO: Add progress dialog here
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
            System.Windows.Application.Current.Shutdown();
        }

        private void refreshTrackHeaders_Click(object sender, RoutedEventArgs e)
        {
            this.refreshTrackHeaders();
        }

        private void HandleTrackHeaderSelection(object sender, SelectionChangedEventArgs args)
        {
            System.Windows.Controls.ListView lv = sender as System.Windows.Controls.ListView;

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
            System.Windows.Controls.ComboBox cb = sender as System.Windows.Controls.ComboBox;

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
            cbItems.Clear();

            var cbItem = new ComboBoxItem { Content = "Not selected" };
            this.SelectedcbItem = cbItem;
            cbItems.Add(cbItem);            

            foreach (string p in this._ports.getPorts())
            {                
                cbItems.Add(new ComboBoxItem { Content = p });
            }
        }

        private void initializeObservers()
        {
            cbItems = new ObservableCollection<ComboBoxItem>();
            TrackHeaders = new ObservableCollection<System.Windows.Controls.ListViewItem>();
        }

        private void refreshTrackHeaders()
        {
            try
            {
                // TODO: Add progress dialog here
                GetDGTrackHeadersCommand c = new GetDGTrackHeadersCommand();
                c.setSerialConnection(this.getSerialConnection());

                c.execute();

                GetDGTrackHeadersCommandResult cr = (GetDGTrackHeadersCommandResult)c.getLastResult();

                this.TrackHeaders.Clear();

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
                this.TrackHeaders.Add(new System.Windows.Controls.ListViewItem { Content = this._currentTrackHeaderEntry });
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
                this.mnuConfig.IsEnabled = true;
                this.mnuErase.IsEnabled = true;

                this.refreshTrackHeaders();
            }
            catch (kimandtodd.DG200CSharp.commands.exceptions.CommandException ex)
            {
                this.lblStatus.Content = "Connection failed: " + ex.Message;
            }
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            string filepath = this.openFileDialog();

            if (filepath != "")
            {
                // TODO: Add progress dialog here
                ISet<System.Windows.Controls.ListViewItem> headers = this.getSelectedTrackHeaders();
                if (headers.Count == 0) { return; }

                GpxSerializer s = new GpxSerializer();
                s.setSerialConnection(this._dgSerialConnection);

                foreach(System.Windows.Controls.ListViewItem sel in headers)
                {
                    TrackHeaderEntry the = sel.Content as TrackHeaderEntry;
                    s.addTrackHeaderEntry(the);
                }

                s.setFilePath(filepath);

                s.serialize();
            }
        }

        private string openFileDialog()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "GPX files|*.gpx";
            saveFileDialog1.Title = "Save the GPX data";
            saveFileDialog1.ShowDialog();
            return saveFileDialog1.FileName;
        }

        private ISet<System.Windows.Controls.ListViewItem> getSelectedTrackHeaders()
        {
            HashSet<System.Windows.Controls.ListViewItem> set = new HashSet<System.Windows.Controls.ListViewItem>();

            foreach (System.Windows.Controls.ListViewItem sel in this.lvTracks.SelectedItems )
            {
                set.Add(sel);
            }

            return set;
        }
    }
}
