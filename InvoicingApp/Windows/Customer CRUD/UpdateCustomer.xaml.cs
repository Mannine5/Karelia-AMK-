using InvoicingApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Interaction logic for UpdateCustomer.xaml
    /// </summary>
    public partial class UpdateCustomer : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly InvoiceRepository repo = new InvoiceRepository();

        public static event Action? CustomerUpdated;
        public ObservableCollection<Customer> Customers { get; set; }

        private Customer? selectedCustomer;
        public Customer? SelectedCustomer
        {
            get
            {
                return selectedCustomer;
            }
            set
            {
                if (selectedCustomer != value)
                {
                    selectedCustomer = value;
                    OnPropertyChanged(nameof(SelectedCustomer));
                }
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public UpdateCustomer() : this(0)
        {
        }

        public UpdateCustomer(int selectedIndex)
        {
            InitializeComponent();
            DataContext = this;

            Customers = repo.GetCustomers();

            // Jos listassa on asiakkaita, valitaan ensimmäinen, muuten SelectedCustomer pysyy null-arvona
            SelectedCustomer = Customers.Count > 0 ? Customers[selectedIndex] : null;
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                SaveClicked(sender, e);
            }
        }

        private void SaveClicked(object sender, RoutedEventArgs e)
        {
            if (SelectedCustomer == null)
            {
                MessageBox.Show("No customer selected or customer list is empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Kutsutaan repo-metodia päivitystä varten
            if (repo.UpdateCustomer(SelectedCustomer))
            {
                MessageBox.Show("Customer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CustomerUpdated?.Invoke();
            }
            else
            {
                MessageBox.Show("Customer update failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
