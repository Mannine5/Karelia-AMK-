using InvoicingApp.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace InvoicingApp
{
    /// <summary>
    /// Interaction logic for DeleteCustomer.xaml
    /// </summary>
    public partial class DeleteCustomer : Window
    {
        private readonly InvoiceRepository repo = new InvoiceRepository();
        private ObservableCollection<Customer> customers = new ObservableCollection<Customer>();

        public static event Action? CustomerDeleted;

        public DeleteCustomer()
        {
            InitializeComponent();
            repo = new InvoiceRepository();
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            customers = repo.GetCustomers(); // Metodi hakee kaikki tuotteet tietokannasta
            CustomerComboBox.ItemsSource = customers;

            if (customers.Count > 0)
            {
                CustomerComboBox.SelectedItem = customers[0];// Asetetaan ensimmäinen tuote valituksi
            }
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                DeleteClicked(sender, e);
            }
        }

        private void DeleteClicked(object sender, RoutedEventArgs e)
        {
            if (CustomerComboBox.SelectedItem is Customer selectedCustomer)
            {
                // Tarkistetaan, onko asiakkaalla laskuja
                int invoiceCount = repo.GetInvoiceCountByCustomer(selectedCustomer.CustomerId);

                if (invoiceCount > 0)
                {
                    MessageBoxResult result = MessageBox.Show(
                        $"This customer has {invoiceCount} invoices. Deleting the customer will also delete all their invoices.\n\nAre you sure?",
                        "Confirm Deletion",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes)
                    {
                        return; // Käyttäjä peruutti poiston
                    }
                }

                // Suoritetaan poisto
                if (repo.DeleteCustomer(selectedCustomer.CustomerId))
                {
                    MessageBox.Show("Customer deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    CustomerDeleted?.Invoke();
                    LoadCustomers(); // Päivitetään lista
                }
                else
                {
                    MessageBox.Show("Failed to delete customer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a customer.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
