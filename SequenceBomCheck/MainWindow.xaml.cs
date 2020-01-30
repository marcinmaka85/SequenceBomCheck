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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SequenceBomCheck
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        #region MENU 
        /// <summary> 
        /// Menu section events and actions
        /// </summary>
        private bool IsStarted = false; //Start clicked and confirmed in the current env ==> true, else ==> false

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            string aboutText = "Author: Marcin Maka \r\n marcin.maka@autoliv.com \r\n +48 783 440 550";
            string aboutHeader = "Version 0.1 ... probably :)";
            MessageBox.Show(aboutText, aboutHeader);
        }
        private void MenuStart_Click(object sender, RoutedEventArgs e)
        {
            if (this.CheckEnviroment() == true)
            {
                this.IsStarted = true;
                //MessageBox.Show("enviroment ok");
                this.RefreshAll();
            }
            else
            {
                string aboutText = "Check if the enviroment is correct!";
                string aboutHeader = "Enviroment Error";
                MessageBox.Show(aboutText, aboutHeader);
                this.IsStarted = false;
                this.RefreshAll();
            }

        }

        private void MenuStop_Click(object sender, RoutedEventArgs e)
        {
            this.IsStarted = false;
            this.RefreshAll();
        }
        #endregion

        #region MAIN WINDOW general events
        /// <summary> 
        /// General Main Window events 
        ///</summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string ExitInfo = "Do you really want to exit?";
            MessageBoxResult result;
            result = MessageBox.Show(ExitInfo, "", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
        #endregion

        #region OPERATIONS
        private void RefreshAll()
        // refresh all displayed items on the main window
        {
            if (this.IsStarted)
            {
                MainContent.IsEnabled = true;
                MenuStart.IsEnabled = false;
                MenuStop.IsEnabled = true;
                DatabaseOperations db = new DatabaseOperations();
                UserPcInfo.Text = "User: " + System.Environment.UserName + " // PC: " + System.Environment.MachineName + " // ATraQ line: " + db.getATraQLineID();
                if (ComboBox_AmoteqLine.SelectedItem == null)
                {
                    ComboBox_AmoteqLine.ItemsSource = db.GetAmoteqLine(db.getATraQLineID());
                    ComboBox_AmoteqLine.SelectedItem = db.getActiveAmoteqLine();
                }

                if (ComboBox_AmoteqLine.SelectedItem != null)
                    ComboBox_RackNo.IsEnabled = true;
                else
                    ComboBox_RackNo.IsEnabled = false;

                if ((ComboBox_RackNo.SelectedItem != null) & (ComboBox_RackNo.IsEnabled == true))
                    ComboBox_PosInRack.IsEnabled = true;
                else
                    ComboBox_PosInRack.IsEnabled = false;

                if ((ComboBox_PosInRack.SelectedItem != null) & (ComboBox_PosInRack.IsEnabled == true))
                    ComboBox_Root.IsEnabled = true;
                else
                    ComboBox_Root.IsEnabled = false;
            }
            else
            {
                MainContent.IsEnabled = false;
                MenuStart.IsEnabled = true;
                MenuStop.IsEnabled = false;
                UserPcInfo.Text = "User: " + System.Environment.UserName + " // PC: " + System.Environment.MachineName + " // ATraQ line: xxxx";
            }
        }

        private bool CheckEnviroment()
        // check DB exists & connection if fine
        {
            bool result = false;
            DatabaseOperations db = new DatabaseOperations();
            if (db.CheckConnection())
            {
                result = true;
            }
            return result;
        }

        private void ComboBox_AmoteqLine_DropDownOpened(object sender, EventArgs e)
        {
            DatabaseOperations db = new DatabaseOperations();
            ComboBox_AmoteqLine.ItemsSource = db.GetAmoteqLine(db.getATraQLineID());

        }

        private void ComboBox_PosInRack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.RefreshAll();
            //cleaning next selection
            ComboBox_Root.SelectedItem = null;
            //cleaning sequence info
            TextBox_id_stocpf.Text = null;
            TextBox_id_trace_order.Text = null;
            TextBox_Quality.Text = null;
            TextBox_stocpf_no_cli.Text = null;
            TextBox_FPPN.Text = null;
            TextBox_OrderBOM_Quantity.Text = null;
        }

        private void ComboBox_AmoteqLine_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.RefreshAll();
            ComboBox_RackNo.SelectedItem = null;
        }

        private void ComboBox_RackNo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.RefreshAll();
            ComboBox_PosInRack.SelectedItem = null;

        }
        private void ComboBox_RackNo_DropDownOpened(object sender, EventArgs e)
        {
            DatabaseOperations db = new DatabaseOperations();
            List<Control> checkboxlist = this.CreateControlList();
            string statuses = this.StatusFilterString(checkboxlist);
            ComboBox_RackNo.ItemsSource = db.getSequnecesRacks(ComboBox_AmoteqLine.SelectedItem.ToString(), statuses);

        }

        private void ComboBox_PosInRack_DropDownOpened(object sender, EventArgs e)
        {
            List<Control> checkboxlist = this.CreateControlList();
            string statuses = this.StatusFilterString(checkboxlist);
            DatabaseOperations db = new DatabaseOperations();
            ComboBox_PosInRack.ItemsSource = db.getRackPositions(ComboBox_AmoteqLine.SelectedItem.ToString(), statuses, ComboBox_RackNo.SelectedItem.ToString());
        }
        private void ComboBox_Root_DropDownOpened(object sender, EventArgs e)
        {
            DatabaseOperations db = new DatabaseOperations();
            ComboBox_Root.ItemsSource = db.getSequenceRoot(ComboBox_AmoteqLine.SelectedItem.ToString(), ComboBox_RackNo.SelectedItem.ToString(), ComboBox_PosInRack.SelectedItem.ToString());
        }

        private void ComboBox_Root_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            AMOTEQBOM.ItemsSource = null;
            if (ComboBox_Root.SelectedItem != null)
            {
                DatabaseOperations db = new DatabaseOperations();
                AMOTEQBOM.ItemsSource = null;
                string RackNo = ComboBox_RackNo.SelectedItem.ToString();
                string RackPos = ComboBox_PosInRack.SelectedItem.ToString();
                string Root = ComboBox_Root.SelectedItem.ToString();
                string id_trace_order = db.getIdTraceOrder(RackNo, RackPos, Root);
                //MessageBox.Show(id_trace_order);
                AMOTEQBOM.ItemsSource = db.getSeqBillOfMaterial(id_trace_order);

                for (int i = 0; i < AMOTEQBOM.Items.Count; i++)
                {
                    //to color the Part Numbers basing on their traceability level
                }

                //DatabaseOperations.PartinBom Selection = (DatabaseOperations.PartinBom)AMOTEQBOM.SelectedItem; //cast selected item into PartInBom type, to read properly
                DatabaseOperations.PartInfo xxx = new DatabaseOperations.PartInfo(id_trace_order);
                TextBox_id_stocpf.Text = "id_stocpf: " + xxx.Part.id_stocpf;
                TextBox_id_trace_order.Text = "id_trace_order: " + xxx.Part.id_trace_order;
                TextBox_Quality.Text = "Quality: " + xxx.Part.Quality;
                TextBox_stocpf_no_cli.Text = "SN: " + xxx.Part.stocpf_no_cli;
                TextBox_FPPN.Text = xxx.Part.FPPN;
                TextBox_OrderBOM_Quantity.Text = "Used quantity: " + xxx.Part.Quantity;
                if (ComboBox_FAMILIY_BOM.SelectedItem != null)
                {
                    ComboBox_Filter.IsEnabled = true;
                }
            }
            else
            {
                ComboBox_Filter.SelectedIndex = 1;
                ComboBox_Filter.IsEnabled = false;
            }



        }

        public string StatusFilterString(List<Control> lista)
        {
            // basing on the status filters, sets a result in form of "('xx','yy')" string
            // the Tag value is taken from xml
            string tmp = "";
            string result = "('";
            bool nothing = true;

            foreach (var ctrl in lista)
            {
                if (((CheckBox)ctrl).IsChecked == true)
                {
                    tmp += ((CheckBox)ctrl).Tag.ToString();
                    result += tmp + "'";
                    nothing = false;
                    tmp = ",'";
                }
            }

            result += ")";

            if (nothing)
            {
                result = "('')";
            }

            return result;
        }

        #endregion

        #region Checkbox list
        /// <summary>
        /// region to manage the checkboxes
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private List<Control> CreateControlList()
        {
            List<Control> result = new List<Control>();
            result.Clear();
            result.Add(CheckboxStatus10);
            result.Add(CheckboxStatus19);
            result.Add(CheckboxStatus20);
            result.Add(CheckboxStatus21);
            result.Add(CheckboxStatusOther);
            return result;
        }

        private void CheckboxStatusAll_Unchecked(object sender, RoutedEventArgs e)
        {
            List<Control> checkboxlist = this.CreateControlList();
            if (CheckboxStatusAll.IsFocused)
            {
                foreach (var ctrl in checkboxlist)
                {
                    if (ctrl.GetType() == typeof(CheckBox))
                    {
                        ((CheckBox)ctrl).IsChecked = false;
                    }
                }
            }
        }

        private void CheckboxStatusAll_Checked(object sender, RoutedEventArgs e)
        {
            List<Control> checkboxlist = this.CreateControlList();
            foreach (var ctrl in checkboxlist)
            {
                if (ctrl.GetType() == typeof(CheckBox))
                {
                    ((CheckBox)ctrl).IsChecked = true;
                }
            }
        }




        #endregion

        private void AMOTEQBOM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


        }

        private void FAMILIYBOM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComboBox_FAMILIY_BOM_DropDownOpened(object sender, EventArgs e)
        {
            DatabaseOperations db = new DatabaseOperations();
            ComboBox_FAMILIY_BOM.ItemsSource = db.getFamilies(db.getATraQLineID());
        }

        private void ComboBox_FAMILIY_BOM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            FAMILIYBOM.ItemsSource = null;
            if (ComboBox_FAMILIY_BOM.SelectedItem != null)
            {
                DatabaseOperations db = new DatabaseOperations();
                FAMILIYBOM.ItemsSource = null;
                FAMILIYBOM.ItemsSource = db.getBillOfMaterial(ComboBox_FAMILIY_BOM.SelectedItem.ToString());
                if (ComboBox_Root.SelectedItem != null)
                {
                    ComboBox_Filter.IsEnabled = true;
                }
            }
            else
            {
                ComboBox_Filter.IsEnabled = false;
                ComboBox_Filter.SelectedIndex = 1;
            }
        }

        private void ComboBox_Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_Filter.SelectedItem.ToString() == "No")
            {
                // show full BOM of familly
                FAMILIYBOM.ItemsSource = null;
                if (ComboBox_FAMILIY_BOM.SelectedItem != null)
                {
                    DatabaseOperations db = new DatabaseOperations();
                    FAMILIYBOM.ItemsSource = null;
                    FAMILIYBOM.ItemsSource = db.getBillOfMaterial(ComboBox_FAMILIY_BOM.SelectedItem.ToString());
                }
            }
            else
            {
                //show only those PN which match PN on the t_trace_order BOM
                DatabaseOperations db = new DatabaseOperations();
                //FAMILIYBOM.ItemsSource = db.getFilteredBOM(TextBox_id_trace_order.Text,ComboBox_FAMILIY_BOM.SelectedItem.ToString());
            }
        }
    }
}

