using InvoicingApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
using System.Windows.Threading;

namespace InvoicingApp
{
    /// <summary>
    /// Interaction logic for UpdateInvoice.xaml
    /// </summary>
    /// 

    public partial class UpdateInvoice : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static event Action? InvoiceUpdated; // Tapahtuma laskun päivitykselle

        private readonly InvoiceRepository repo = new(); // Tietokantayhteys

        private ObservableCollection<Invoice> Invoices = new(); // Lista laskuista

        private ObservableCollection<Product> Products = new(); // Lista tuotteista
        public ObservableCollection<InvoiceRow> ProductRows { get; } = new(); // Tuoterivit
        public ObservableCollection<InvoiceRow> WorkRows { get; } = new(); // Työrivit

        private Customer originalCustomer = new Customer(); // Alkuperäinen asiakas tallennetaan vertailua varten

        private Invoice? displayedInvoice;
        public Invoice? DisplayedInvoice
        {
            get => displayedInvoice;
            set
            {
                if (displayedInvoice != value)
                {
                    displayedInvoice = value;
                    OnPropertyChanged(nameof(DisplayedInvoice));
                    UpdateFilteredLists(); // Päivitetään laskurivit, kun lasku vaihtuu
                    UpdateTotalPrice(); // Päivitetään myös TotalPrice automaattisesti
                }
            }
        }

        private decimal totalPrice;
        public decimal TotalPrice
        {
            get { return totalPrice; }
            private set
            {
                if (totalPrice != value)
                {
                    totalPrice = value;
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }

        // Parametritön konstruktori
        public UpdateInvoice() : this(0)
        {
        }

        // Parametrillinen konstruktori, joka vastaanottaa laskun indeksin

        public UpdateInvoice(int selectedIndex)
        {
            InitializeComponent();
            DataContext = this;
            Loaded += async (sender, e) => await LoadDataAsync(selectedIndex);

        }

        /// <summary>
        /// Lataa tarvittavat tiedot tietokannasta asynkronisesti
        /// </summary>
        private async Task LoadDataAsync(int selectedIndex)
        {
            // Aluksi näytetään latausviesti
            MainContent.Visibility = Visibility.Collapsed;
            LoadingMessage.Visibility = Visibility.Visible;

            try
            {
                Products = await Task.Run(() => repo.GetProducts());
                Invoices = await Task.Run(() => repo.GetInvoices());

                // UI-päivitys Dispatcherin sisällä
                Dispatcher.Invoke(() =>
                {
                    InvoiceComboBox.ItemsSource = Invoices;
                    ProductComboBox.ItemsSource = Products;

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
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading data: {ex.Message}");
            }

            LoadingMessage.Visibility = Visibility.Collapsed;
            MainContent.Visibility = Visibility.Visible;

        }

        // Käsittelee laskun valinnan pudotusvalikosta
        private void InvoiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMainContent(); // Päivittää käyttöliittymän ja näyttää valitun laskun tiedot
        }

        /// <summary>
        /// Päivittää näkyvän sisällön valitun laskun perusteella
        /// </summary>
        private void UpdateMainContent()
        {
            if (Invoices.Count != 0)
            {
                NoInvoicesMessage.Visibility = Visibility.Collapsed;
                MainContent.Visibility = Visibility.Visible;


                if (InvoiceComboBox.SelectedItem is Invoice selectedInvoice)
                {

                    DisplayedInvoice = selectedInvoice;

                    // Tee kopio alkuperäisestä asiakkaasta
                    originalCustomer = new Customer
                    {
                        CustomerId = selectedInvoice.Customer.CustomerId,
                        Name = selectedInvoice.Customer.Name,
                        StreetAddress = selectedInvoice.Customer.StreetAddress,
                        PostalCode = selectedInvoice.Customer.PostalCode,
                        City = selectedInvoice.Customer.City
                    };

                    UpdateFilteredLists();
                    UpdateTotalPrice();
                }
            }

            else
            {
                MainContent.Visibility = Visibility.Collapsed;
                NoInvoicesMessage.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Päivittää tuote- ja työrivit
        /// </summary>
        public void UpdateFilteredLists()
        {
            if (DisplayedInvoice == null) return;

            ProductRows.Clear();
            WorkRows.Clear();

            foreach (InvoiceRow row in DisplayedInvoice.Rows)
            {
                (row.RowType == "Product" ? ProductRows : WorkRows).Add(row);
            }
        }

        private decimal CalculateTotalPrice()
        {
            return ProductRows.Concat(WorkRows).Sum(row => row.Quantity * row.UnitPrice + row.HourCost * row.WorkHours);
        }

        private void UpdateTotalPrice()
        {
            TotalPrice = CalculateTotalPrice();
            OnPropertyChanged(nameof(TotalPrice));
        }

        /// <summary>
        /// Käsittelee tuotetaulukon solun muokkaamisen
        /// </summary>
        private void ProductGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Debug.WriteLine("Call ProductRow");

            if (e.Row.Item is InvoiceRow row)
            {
                if (e.Column is DataGridComboBoxColumn && e.EditingElement is ComboBox comboBox)
                {

                    if (comboBox.SelectedValue is int selectedProductId)
                    {
                        Product? selectedProduct = Products.FirstOrDefault(p => p.ProductId == selectedProductId);
                        if (selectedProduct != null)
                        {
                            row.ProductId = selectedProduct.ProductId;
                            row.ProductName = selectedProduct.Name;
                            row.Description = selectedProduct.Description;
                            row.UnitPrice = selectedProduct.Price ?? 0;
                            row.Quantity = 1;
                        }
                    }
                }

                if (e.EditingElement is TextBox textBox)
                {
                    string? columnName = e.Column.Header.ToString();
                    string input = textBox.Text.Trim();

                    switch (columnName)
                    {
                        case "Description":
                            row.Description = input; // Poistetaan ylimääräiset välilyönnit
                            break;

                        case "Unit Price":
                            input = input.Replace(',', '.'); // Muutetaan pilkku pisteeksi
                            if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                            {
                                row.UnitPrice = price;
                            }
                            else
                            {
                                MessageBox.Show("Invalid input. Please enter a valid number for Price.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                                textBox.Text = "";
                            }
                            break;

                        case "Quantity":
                            if (int.TryParse(input, out int quantity))
                            {
                                row.Quantity = quantity;
                            }
                            else
                            {
                                MessageBox.Show("Invalid input. Please enter a valid integer number for Quantity.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                                textBox.Text = "";
                            }
                            break;
                    }
                }
            }

            // Viivästetään TotalPrice-laskentaa, jotta DataGrid ehtii päivittää arvot
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Debug.WriteLine("ProductGrid rows: ");

                foreach (InvoiceRow r in ProductRows)
                {
                    Debug.WriteLine($"{r.ProductName} {r.UnitPrice} {r.Quantity}");
                }

                UpdateTotalPrice();
            }), DispatcherPriority.Background);
        }

        private void WorkGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

            Debug.WriteLine("Call WorkRow");

            if (e.Row.Item is InvoiceRow row && e.EditingElement is TextBox textBox)
            {
                string? columnName = e.Column.Header.ToString();
                string input = textBox.Text.Trim();

                switch (columnName)
                {
                    case "Work Description":
                        row.Description = input; // Poistetaan ylimääräiset välilyönnit
                        break;

                    case "Hour Cost":
                        input = textBox.Text.Replace(',', '.'); // Muutetaan pilkku pisteeksi
                        if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal hourCost))
                        {
                            row.HourCost = hourCost;
                        }
                        else
                        {
                            MessageBox.Show("Invalid input. Please enter a valid number for Hour Cost.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            textBox.Text = "";
                        }
                        break;

                    case "Work Hours":
                        if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal workHours))
                        {
                            row.WorkHours = workHours;
                        }
                        else
                        {
                            MessageBox.Show("Invalid input. Please enter a valid number for Work Hours.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            textBox.Text = row.HourCost.ToString(CultureInfo.InvariantCulture);
                        }
                        break;
                }
            }

            // Viivästetään TotalPrice-laskentaa, jotta DataGrid ehtii päivittää arvot
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Debug.WriteLine("WorkGrid rows: ");

                foreach (InvoiceRow row in WorkRows)
                {
                    Debug.WriteLine($"{row.Description} {row.HourCost} {row.WorkHours}");
                }
                UpdateTotalPrice();
            }), DispatcherPriority.Background);
        }

        private void DeleteProductRow(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is InvoiceRow row)
            {
                ProductRows.Remove(row);  // Poistaa rivin ObservableCollectionista
            }

            // Jos listassa ei ole rivejä jäljellä, lisää uusi tyhjä rivi
            if (ProductRows.Count == 0)
            {
                ProductRows.Add(new InvoiceRow());
            }
            UpdateTotalPrice();
        }

        private void DeleteWorkRow(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is InvoiceRow row)
            {
                WorkRows.Remove(row);  // Poistaa rivin ObservableCollectionista
            }

            // Jos listassa ei ole rivejä jäljellä, lisää uusi tyhjä rivi
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (WorkRows.Count == 0)
                {
                    WorkRows.Add(new InvoiceRow());
                }
            }), DispatcherPriority.Background);
            UpdateTotalPrice();
        }

        private void ProductGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ProductRows.Add(new InvoiceRow());

                }), DispatcherPriority.Background);
            }
        }

        private void WorkGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                WorkRows.Add(new InvoiceRow());
            }
        }

        /// <summary>
        /// Tallentaa laskun muutokset tietokantaan
        /// </summary>
        private void SaveClicked(object sender, RoutedEventArgs e)
        {
            if (DisplayedInvoice == null)
            {
                MessageBox.Show("No invoice selected!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Tarkistetaan, ovatko asiakkaan tiedot muuttuneet alkuperäiseen verrattuna
            bool customerChanged =
                originalCustomer.Name != CustomerNameTextBox.Text ||
                originalCustomer.StreetAddress != StreetAddressTextBox.Text ||
                originalCustomer.PostalCode != PostalCodeTextBox.Text ||
                originalCustomer.City != CityTextBox.Text;

            if (customerChanged)
            {
                var result = MessageBox.Show("You have modified the customer's details. These changes will be applied to all invoices associated with this customer and their customer information.",
                                              "Confirm Update",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return; // Käyttäjä perui tallennuksen
                }

                // Päivitetään asiakkaan tiedot
                DisplayedInvoice.Customer.Name = CustomerNameTextBox.Text;
                DisplayedInvoice.Customer.StreetAddress = StreetAddressTextBox.Text;
                DisplayedInvoice.Customer.PostalCode = PostalCodeTextBox.Text;
                DisplayedInvoice.Customer.City = CityTextBox.Text;

                // Päivitetään asiakas tietokantaan
                repo.UpdateCustomer(DisplayedInvoice.Customer);
            }

            // Päivitetään laskun tiedot
            DisplayedInvoice.InvoiceDate = InvoiceDatePicker.SelectedDate ?? DateTime.Now;
            DisplayedInvoice.DueDate = DueDatePicker.SelectedDate ?? DateTime.Now;
            DisplayedInvoice.Notes = NotesTextBox.Text;

            // Päivitetään laskurivit (tyhjennetään ja lisätään molemmista DataGridistä uudet rivit)
            DisplayedInvoice.Rows.Clear();

            // Haetaan rivit tuotetaulukosta
            foreach (InvoiceRow row in ProductRows)
            {
                row.RowType = "Product"; // Merkitään tuotetyypiksi
                DisplayedInvoice.Rows.Add(row);
            }

            // Haetaan rivit työtaulukosta
            foreach (InvoiceRow row in WorkRows)
            {
                row.RowType = "Work"; // Merkitään työtyypiksi
                DisplayedInvoice.Rows.Add(row);
            }

            Debug.WriteLine("Saving invoice with rows:");
            foreach (var row in DisplayedInvoice.Rows)
            {
                Debug.WriteLine($"Row: Description={row.Description}, HourCost={row.HourCost} WorkHours={row.WorkHours}");
            }

            // Kutsutaan tietokantapäivitystä
            bool success = repo.UpdateInvoice(DisplayedInvoice);

            if (success)
            {
                MessageBox.Show("Invoice updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                // Kutsutaan tapahtumaa
                InvoiceUpdated?.Invoke();
            }
            else
            {
                MessageBox.Show("Failed to update invoice.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
