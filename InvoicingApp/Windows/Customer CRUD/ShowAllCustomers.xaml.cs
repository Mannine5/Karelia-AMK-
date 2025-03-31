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

namespace InvoicingApp
{
    /// <summary>
    /// Interaction logic for ShowAllCustomers.xaml
    /// </summary>
    public partial class ShowAllCustomers : Window
    {
        private readonly Action updateCustomersHandler;

        private readonly InvoiceRepository repo = new InvoiceRepository();
        public ObservableCollection<Customer> Customers = new();

        public ShowAllCustomers()
        {
            InitializeComponent();
            Loaded += async (sender, e) => await LoadDataAsync();
            updateCustomersHandler = async () => { await LoadDataAsync(); };

            Closed += Window_Closed!;
            SubscribeToEvents();
        }

        private async Task LoadDataAsync()
        {
            CustomersDataGrid.Visibility = Visibility.Collapsed;
            LoadingMessage.Visibility = Visibility.Visible;

            try
            {
                Customers = await Task.Run(() => repo.GetCustomers());
                CustomersDataGrid.ItemsSource = Customers;
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading data: {ex.Message}");
            }

            LoadingMessage.Visibility = Visibility.Collapsed;
            CustomersDataGrid.Visibility = Customers.Any() ? Visibility.Visible : Visibility.Collapsed;
            NoCustomersMessage.Visibility = Customers.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SubscribeToEvents()
        {
            CreateNewCustomer.CustomerCreated += updateCustomersHandler;
            UpdateCustomer.CustomerUpdated += updateCustomersHandler;
            DeleteCustomer.CustomerDeleted += updateCustomersHandler;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CreateNewCustomer.CustomerCreated -= updateCustomersHandler;
            UpdateCustomer.CustomerUpdated -= updateCustomersHandler;
            DeleteCustomer.CustomerDeleted -= updateCustomersHandler;
        }

        private void CustomersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CustomersDataGrid.SelectedItem is Customer selectedCustomer)
            {
                // Etsi laskun indeksi ja välitä se MainWindow:iin
                int selectedIndex = Customers.IndexOf(selectedCustomer);

                UpdateCustomer updateCustomer = new UpdateCustomer();
                updateCustomer.Show();

            }
        }
    }
}