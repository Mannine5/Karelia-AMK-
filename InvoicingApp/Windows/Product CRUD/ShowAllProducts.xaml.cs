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
    /// Interaction logic for ShowAllProducts.xaml
    /// </summary>
    public partial class ShowAllProducts : Window
    {
        private readonly Action updateProductsHandler;

        private readonly InvoiceRepository repo = new InvoiceRepository();
        public ObservableCollection<Product> Products = new();

        public ShowAllProducts()
        {
            InitializeComponent();
            Loaded += async (sender, e) => await LoadDataAsync();
            updateProductsHandler = async () => { await LoadDataAsync(); };

            Closed += Window_Closed!;
            SubscribeToEvents();
        }

        private async Task LoadDataAsync()
        {
            ProductsDataGrid.Visibility = Visibility.Collapsed;
            LoadingMessage.Visibility = Visibility.Visible;

            try
            {
                Products = await Task.Run(() => repo.GetProducts());
                ProductsDataGrid.ItemsSource = Products;
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading data: {ex.Message}");
            }

            LoadingMessage.Visibility = Visibility.Collapsed;
            ProductsDataGrid.Visibility = Products.Any() ? Visibility.Visible : Visibility.Collapsed;
            NoProductsMessage.Visibility = Products.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SubscribeToEvents()
        {
            CreateNewProduct.ProductCreated += updateProductsHandler;
            UpdateProduct.ProductUpdated += updateProductsHandler;
            DeleteProduct.ProductDeleted += updateProductsHandler;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CreateNewProduct.ProductCreated -= updateProductsHandler;
            UpdateProduct.ProductUpdated -= updateProductsHandler;
            DeleteProduct.ProductDeleted -= updateProductsHandler;
        }

        private void ProductsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Product selectedProduct)
            {
                // Etsi laskun indeksi ja välitä se MainWindow:iin
                int selectedIndex = Products.IndexOf(selectedProduct);

                UpdateProduct updateProduct = new UpdateProduct(selectedIndex);
                updateProduct.Show();

            }
        }
    }
}
