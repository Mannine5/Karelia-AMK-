using InvoicingApp.Models;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for CreateNewProduct.xaml
    /// </summary>
    public partial class CreateNewProduct : Window
    {
        public static event Action? ProductCreated; 

        private readonly InvoiceRepository repo = new InvoiceRepository();
        private readonly Product product = new Product();

        public CreateNewProduct()
        {

            InitializeComponent();
            DataContext = product;

        }
   
        private void PriceBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PriceBox.Text))
            {
                PriceBox.Text = PriceBox.Text.Replace(',', '.');
            }
        }

        private bool PriceValidation()
        {
            // Tarkistetaan, onko kenttä tyhjä
            if (string.IsNullOrWhiteSpace(PriceBox.Text))
            {
                product.Price = 0m;
                return true;
            }

            try
            {
                // Yritetään muuntaa käyttäjän syöte desimaaliluvuksi
                decimal price = Convert.ToDecimal(PriceBox.Text, System.Globalization.CultureInfo.InvariantCulture);

                // Tarkistetaan, että hinta on sallitulla alueella
                if (price > 999999.99m || price < -999999.99m)
                {
                    MessageBox.Show("The price is out of range.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Pyöristetään arvo kahden desimaalin tarkkuuteen
                product.Price = Math.Round(price, 2);

                return true;
            }
            catch (FormatException)
            {
                // Näytetään virheilmoitus, jos syöte ei ole numero
                MessageBox.Show("Please enter a valid numeric price.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (Exception ex)
            {
                // Käsitellään muut mahdolliset virheet ja tulostetaan ne
                Debug.WriteLine($"Error: {ex.Message}");
                MessageBox.Show("An unexpected error occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                PriceBox_LostFocus(sender, e);
                SaveClicked(sender, e); 
            }
        }

        private void SaveClicked(object sender, RoutedEventArgs e)
        {

            // Päivitetään olion ominaisuuden arvo manuaalisesti. Tässä tapauksessa Price.
            // Jos käytettäisiin UpdateSourceTrigger=PropertyChanged, nii syöte ei hyväksy pisteitä eikä pilkkuja.
            PriceBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

            if (!PriceValidation()) return;

            // Tarkistetaan, että hinta on sallittujen rajojen sisällä
            if (product.Price > 999999.99m || product.Price < -999999.99m)
            {
                MessageBox.Show("The price is out of range.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (repo.CreateProduct(product))
            {
                MessageBox.Show("Product saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ProductCreated?.Invoke();
                Close();
            }
            else
            {
                MessageBox.Show("Product saving failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
