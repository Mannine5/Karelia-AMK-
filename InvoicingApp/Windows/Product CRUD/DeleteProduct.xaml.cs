using InvoicingApp.Models;
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
using System.Windows.Shapes;

namespace InvoicingApp
{
    /// <summary>
    /// Interaction logic for DeleteProduct.xaml
    /// </summary>

    public partial class DeleteProduct : Window
    {
        public static event Action? ProductDeleted;

        private readonly InvoiceRepository repo;
        private ObservableCollection<Product> products = new ObservableCollection<Product>();

        public DeleteProduct()
        {
            InitializeComponent();
            repo = new InvoiceRepository();
            LoadProducts();

        }

        private void LoadProducts()
        {
            products = repo.GetProducts(); // Metodi hakee kaikki tuotteet tietokannasta
            ProductComboBox.ItemsSource = products;

            if (products.Count > 0)
            {
                ProductComboBox.SelectedItem = products[0];// Asetetaan ensimmäinen tuote valituksi
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
            if (ProductComboBox.SelectedItem is Product selectedProduct)
            {
                if (MessageBox.Show(
                    $"Are you sure you want to delete '{selectedProduct.Name}'?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    return; // Käyttäjä peruutti poiston
                }

                if (repo.DeleteProduct(selectedProduct.ProductId))
                {
                    MessageBox.Show("Product deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ProductDeleted?.Invoke();
                    LoadProducts(); 
                }
                else
                {
                    MessageBox.Show("Failed to delete product.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a product.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
