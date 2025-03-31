using InvoicingApp.Models;
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

namespace InvoicingApp
{
    /// <summary>
    /// Interaction logic for CreateNewCustomer.xaml
    /// </summary>
    public partial class CreateNewCustomer : Window
    {
        public static event Action? CustomerCreated;

        private readonly InvoiceRepository repo = new InvoiceRepository();
        private readonly Customer customer = new Customer();

        public CreateNewCustomer()
        {
            InitializeComponent();
            DataContext = customer;
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
            if (repo.CreateCustomer(customer))
            {
                MessageBox.Show("Customer saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CustomerCreated?.Invoke();
                Close();
            }
            else
            {
                MessageBox.Show("Customer saving failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
