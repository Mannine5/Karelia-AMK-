using InvoicingApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using InvoicingApp.Windows;

namespace InvoicingApp
{
    /// <summary>
    /// Interaction logic for ShowAllInvoices.xaml
    /// </summary>
    public partial class ShowAllInvoices : Window
    {
        private readonly Action updateInvoicesHandler;

        private readonly InvoiceRepository repo = new InvoiceRepository();
        public ObservableCollection<Invoice> Invoices = new();
        private MainWindow mainWindow; // Viite MainWindow:iin

        public ShowAllInvoices(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            Loaded += async (sender, e) => await LoadDataAsync();
            updateInvoicesHandler = async () => { await LoadDataAsync(); };

            Closed += Window_Closed!;
            SubscribeToEvents(); 
        }

        private async Task LoadDataAsync()
        {
            InvoicesDataGrid.Visibility = Visibility.Collapsed;
            LoadingMessage.Visibility = Visibility.Visible;

            try
            {
                Invoices = await Task.Run(() => repo.GetInvoices());
                InvoicesDataGrid.ItemsSource = Invoices;
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading data: {ex.Message}");
            }

            LoadingMessage.Visibility = Visibility.Collapsed;
            InvoicesDataGrid.Visibility = Invoices.Any() ? Visibility.Visible : Visibility.Collapsed;
            NoInvoicesMessage.Visibility = Invoices.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SubscribeToEvents()
        {
            UpdateInvoice.InvoiceUpdated += updateInvoicesHandler;
            CreateNewInvoice.InvoiceCreated += updateInvoicesHandler;
            DeleteInvoice.InvoiceDeleted += updateInvoicesHandler;
            DeleteCustomer.CustomerDeleted += updateInvoicesHandler;
            UpdateCustomer.CustomerUpdated += updateInvoicesHandler;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Poistetaan tapahtumat ilman 'await'
            UpdateInvoice.InvoiceUpdated -= updateInvoicesHandler;
            CreateNewInvoice.InvoiceCreated -= updateInvoicesHandler;
            DeleteInvoice.InvoiceDeleted -= updateInvoicesHandler;
            DeleteCustomer.CustomerDeleted -= updateInvoicesHandler;
            UpdateCustomer.CustomerUpdated -= updateInvoicesHandler;
        }

        private void InvoicesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (InvoicesDataGrid.SelectedItem is Invoice selectedInvoice)
            {
                // Etsi laskun indeksi ja välitä se MainWindow:iin
                int selectedIndex = Invoices.IndexOf(selectedInvoice);
                mainWindow.selectedIndex = selectedIndex;  // Asetetaan MainWindow:in selectedIndex
                mainWindow.UpdateMainContent();  // Kutsutaan UpdateMainContent-metodia
            }
        }
    }
}
