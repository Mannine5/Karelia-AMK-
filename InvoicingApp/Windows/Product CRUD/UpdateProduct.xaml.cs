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
    /// Interaction logic for UpdateProduct.xaml
    /// </summary>
    public partial class UpdateProduct : Window, INotifyPropertyChanged
    {
        public static event Action? ProductUpdated;

        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly InvoiceRepository repo = new InvoiceRepository();
        public ObservableCollection<Product> Products { get; set; }

        private Product selectedProduct;
        public Product SelectedProduct
        {
            get
            {
                return selectedProduct;
            }
            set
            {
                if (selectedProduct != value)
                {
                    selectedProduct = value;
                    OnPropertyChanged(nameof(SelectedProduct));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public UpdateProduct() : this(0)
        {
        }

        public UpdateProduct(int selectedIndex)
        {
            InitializeComponent();
            Products = repo.GetProducts();
            DataContext = this;

            // Alustetaan valittu tuote-olio
            selectedProduct = Products.Count > 0 ? Products[selectedIndex] : new Product();

            Debug.WriteLine(selectedProduct.Price);
        }

        private void PriceBox_lostFocus(object sender, RoutedEventArgs e)
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
                SelectedProduct.Price = 0m;
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
                SelectedProduct.Price = Math.Round(price, 2);

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
                PriceBox_lostFocus(sender, e);
                SaveClicked(sender, e);
            }
        }

        private void SaveClicked(object sender, RoutedEventArgs e)
        {
            if (selectedProduct != null)
            {
                // XAML:n puolella olis mahdollista käyttää, UpdateSourceTrigger=PropertyChanged -ominaisuutta, 
                // mutta silloin Comboboxissa oleva tuotteen nimikin päivittyisi jo uutta nimeä kirjoittaessa.
                NameBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

                // Päivitetään olion ominaisuuden arvo manuaalisesti. Tämä osaa nostattaa binding-virheen, mikäli syöte on väärässä muodossa. Kenttä korostuu punaisella.
                // Tätä ennen laukeaa PriceBox_lostFocus() -metodi, joka käsittelee pisteet ja pilkut syötteestä.
                PriceBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

                // Validoidaan päivittämisen jälkeen, jotta ohjelma osaa maalata virheellisen kentän punaiseksi heti kun "Save" on painettu, jos syöte ei vastaa tietotyyppiä.
                if (!PriceValidation()) return;

                // Päivitetään Description
                DescriptionBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

                // Kutsutaan repo-metodia tallennusta varten
                if (repo.UpdateProduct(SelectedProduct))
                {
                    MessageBox.Show("Product updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ProductUpdated?.Invoke();
                }
                else
                {
                    MessageBox.Show("Product update failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a product to update.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
