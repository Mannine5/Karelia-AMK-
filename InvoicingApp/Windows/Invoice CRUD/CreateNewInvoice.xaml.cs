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
    /// Interaction logic for CreateNewInvoice.xaml
    /// </summary>
    public partial class CreateNewInvoice : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static event Action? InvoiceCreated;

        InvoiceRepository repo = new InvoiceRepository();

        private ObservableCollection<Customer> Customers = new();

        private ObservableCollection<Product> Products = new();

        public ObservableCollection<InvoiceRow> ProductRows { get; } = new();

        public ObservableCollection<InvoiceRow> WorkRows { get; } = new();

        private Customer newCustomer = new();
        public Customer NewCustomer
        {
            get { return newCustomer; }
            set
            {
                if (newCustomer != value)
                {
                    newCustomer = value;
                    OnPropertyChanged(nameof(NewCustomer));
                }
            }
        }

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

                    if (selectedCustomer != null)
                    {
                        // Kopioi tiedot NewCustomeriin
                        NewCustomer = new Customer
                        {
                            CustomerId = selectedCustomer.CustomerId,
                            Name = selectedCustomer.Name,
                            StreetAddress = selectedCustomer.StreetAddress,
                            PostalCode = selectedCustomer.PostalCode,
                            City = selectedCustomer.City,
                        };

                        OnPropertyChanged(nameof(NewCustomer));
                    }
                }
            }
        }

        private Invoice newInvoice = new Invoice
        {
            InvoiceDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(14),
            Customer = new Customer(),
            Rows = new ObservableCollection<InvoiceRow>()
        };

        public Invoice NewInvoice
        {
            get { return newInvoice; }
            set
            {
                if (newInvoice != value)
                {
                    newInvoice = value;
                    OnPropertyChanged(nameof(NewInvoice));
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

        public CreateNewInvoice()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += async (sender, e) => await LoadDataAsync();

            // Tilataan CustomerUpdated-tapahtuma UpdateCustomer-luokasta
            UpdateCustomer.CustomerUpdated += async () => await LoadDataAsync();

        }

        private async Task LoadDataAsync()
        {
            try
            {
                Customers = await Task.Run(() => repo.GetCustomers());
                Products = await Task.Run(() => repo.GetProducts());

                // UI-päivitys Dispatcherin sisällä
                Dispatcher.Invoke(() =>
                {
                    CustomerComboBox.ItemsSource = Customers;
                    ProductComboBox.ItemsSource = Products;
                    NewCustomer = new Customer(); // Tämä tyhjentää kaikki asiakastietokentät, jos CustomerUpdated eventti laukeaa.

                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading data: {ex.Message}");
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



        private void SaveClicked(object sender, RoutedEventArgs e)
        {
            // Jos asiakas on valittu ComboBoxista, tarkistetaan asiakastiedot
            if (SelectedCustomer != null)
            {
                // Vertaile valitun asiakkaan tietoja muokattuihin asiakastietoihin
                bool detailsMatch = SelectedCustomer.Name.Equals(NewCustomer.Name) &&
                                    SelectedCustomer.StreetAddress == NewCustomer.StreetAddress &&
                                    SelectedCustomer.PostalCode == NewCustomer.PostalCode &&
                                    SelectedCustomer.City == NewCustomer.City;

                // Jos tiedot eroavat, näytetään varoitus
                if (!detailsMatch)
                {
                    var result = MessageBox.Show(
                        "The customer details have been changed. Do you want to create a new customer?",
                        "Customer Details Changed",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                }
            }

            var existingCustomer = Customers.FirstOrDefault(c =>
                c.Name.Equals(NewCustomer.Name, StringComparison.OrdinalIgnoreCase));

            if (existingCustomer != null)
            {
                bool detailsMatch = existingCustomer.StreetAddress == NewCustomer.StreetAddress &&
                                    existingCustomer.PostalCode == NewCustomer.PostalCode &&
                                    existingCustomer.City == NewCustomer.City;
            }

            // Jos asiakas on uusi, luodaan se tietokannassa
            if (NewCustomer.CustomerId == 0) // Asiakas on uusi, ei ole ID:tä
            {
                if (!repo.CreateCustomer(NewCustomer))
                {
                    MessageBox.Show("Failed to create new customer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Customers.Add(NewCustomer);
            }

            // Varmistetaan, että laskulla on asiakas
            NewInvoice.Customer = NewCustomer;

            // Päivitetään laskun tiedot UI:sta ennen tallennusta**
            NewInvoice.InvoiceDate = InvoiceDatePicker.SelectedDate ?? DateTime.Today;
            NewInvoice.DueDate = DueDatePicker.SelectedDate ?? DateTime.Today.AddDays(14);
            NewInvoice.Notes = NotesTextBox.Text;

            // Lisää laskurivit ennen tallennusta
            NewInvoice.Rows.Clear(); // Varmista, ettei vanhoja rivejä ole
            foreach (InvoiceRow row in ProductRows)
            {
                row.RowType = "Product";
                NewInvoice.Rows.Add(row);
            }

            foreach (InvoiceRow row in WorkRows)
            {
                row.RowType = "Work";
                NewInvoice.Rows.Add(row);
            }

            if (!repo.CreateInvoice(NewInvoice))
            {
                MessageBox.Show("Failed to save invoice.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Invoice saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            InvoiceCreated?.Invoke();
            Close();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }


    }
}
