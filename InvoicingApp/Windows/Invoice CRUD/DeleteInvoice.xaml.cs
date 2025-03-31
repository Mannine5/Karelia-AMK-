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
    /// Interaction logic for DeleteInvoice.xaml
    /// </summary>
    public partial class DeleteInvoice : Window
    {
        private readonly InvoiceRepository repo = new();
        private ObservableCollection<Invoice> Invoices = new();

        // 🔥 Event, joka ilmoittaa laskun poistosta
        public static event Action? InvoiceDeleted;

        public DeleteInvoice() : this(0)
        {

        }

        public DeleteInvoice(int selectedIndex)
        {
            InitializeComponent();
            Loaded += async (sender, e) => await LoadDataAsync(selectedIndex);
        }

        private async Task LoadDataAsync(int selectedIndex)
        {
            MainContent.Visibility = Visibility.Collapsed;
            LoadingMessage.Visibility = Visibility.Visible;

            try
            {

                Invoices = await Task.Run(() => repo.GetInvoices());

                InvoiceComboBox.ItemsSource = Invoices;
                InvoiceComboBox.SelectedItem = Invoices.FirstOrDefault();

                // Asetetaan valittu lasku, jos indeksi on validi
                bool invoiceSelected = false;
                if (Invoices.Count > selectedIndex)
                {
                    InvoiceComboBox.SelectedItem = Invoices[selectedIndex];
                    invoiceSelected = true;
                }

                if (!invoiceSelected)
                {
                    UpdateMainContent();
                }

            }

            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading data: {ex.Message}");
            }

            LoadingMessage.Visibility = Visibility.Collapsed;
            MainContent.Visibility = Invoices.Any() ? Visibility.Visible : Visibility.Collapsed;
            NoInvoicesMessage.Visibility = Invoices.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void InvoiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMainContent();
        }

        private void UpdateMainContent()
        {
            if (InvoiceComboBox.SelectedItem is Invoice selectedInvoice)
            {
                MainContent.Content = new InvoiceView(selectedInvoice);
                NoInvoicesMessage.Visibility = Visibility.Collapsed;
                MainContent.Visibility = Visibility.Visible;
            }
            else
            {
                MainContent.Visibility = Visibility.Collapsed;
                NoInvoicesMessage.Visibility = Visibility.Visible;
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
            if (InvoiceComboBox.SelectedItem is not Invoice selectedInvoice || !selectedInvoice.InvoiceId.HasValue)
            {
                MessageBox.Show("Invalid invoice selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show(
                    $"Are you sure you want to delete invoice 'ID {selectedInvoice.InvoiceId}: {selectedInvoice.Customer.Name}'?",
                    "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            if (repo.DeleteInvoice(selectedInvoice.InvoiceId.Value))
            {
                MessageBox.Show("Invoice deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // 🔥 Poistetaan lasku ObservableCollectionista, jotta UI päivittyy
                Invoices.Remove(selectedInvoice);

                // 🔥 Kutsutaan tapahtumaa, jotta muut ikkunat voivat päivittyä
                InvoiceDeleted?.Invoke();

                // Päivitetään valikko
                InvoiceComboBox.SelectedItem = Invoices.FirstOrDefault();
                UpdateMainContent();
            }
            else
            {
                MessageBox.Show("Failed to delete invoice.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
