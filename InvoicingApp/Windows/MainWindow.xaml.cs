using InvoicingApp.Models;
using Microsoft.VisualBasic;
using MySqlX.XDevAPI.Relational;
using Org.BouncyCastle.Utilities;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace InvoicingApp.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Action updateInvoicesHandler;

        public List<Window> openWindows = new List<Window>();

        private readonly InvoiceRepository repo = new InvoiceRepository();
        public ObservableCollection<Invoice> Invoices = new();
        private InvoiceView? invoiceView;
        public int selectedIndex = 0;

       public MainWindow()
        {
            InitializeComponent();

            // Määritellään tapahtumakäsittelijä päivityksille
            updateInvoicesHandler = async () => 
            { 
                Invoices = await Task.Run(() => { return repo.GetInvoices(); }); 
                UpdateMainContent(); 
            };

            // Käsitellään ikkunan sulkeminen
            Closed += MainWindow_Closed!;

            // Ladataan dataa asynkronisesti ikkunan latautuessa
            Loaded += async (sender, e) =>
            {
                await LoadDataAsync();
                SubscribeToEvents();
                // Tilataan tapahtumat vasta, kun data on ladattu
            };
        }

        // Tilataan eri komponenttien lähettämät tapahtumat
        private void SubscribeToEvents()
        {
            UpdateInvoice.InvoiceUpdated += updateInvoicesHandler;
            CreateNewInvoice.InvoiceCreated += updateInvoicesHandler;
            DeleteInvoice.InvoiceDeleted += updateInvoicesHandler;
            DeleteCustomer.CustomerDeleted += updateInvoicesHandler;
            UpdateCustomer.CustomerUpdated += updateInvoicesHandler;
        }

        // Käsitellään pääikkunan sulkeminen
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // Suljetaan kaikki avoinna olevat ikkunat
            foreach (Window window in openWindows)
            {
                window.Close();
            }

            // Poistetaan tilatut tapahtumat
            UpdateInvoice.InvoiceUpdated -= updateInvoicesHandler;
            CreateNewInvoice.InvoiceCreated -= updateInvoicesHandler;
            DeleteInvoice.InvoiceDeleted -= updateInvoicesHandler;
            DeleteCustomer.CustomerDeleted -= updateInvoicesHandler;
            UpdateCustomer.CustomerUpdated -= updateInvoicesHandler;
        }

        // Ladataan data asynkronisesti
        private async Task LoadDataAsync()
        {
            // Näytetään latausviesti
            MainContent.Visibility = Visibility.Collapsed;
            LoadingMessage.Visibility = Visibility.Visible;

            // Luodaan tietokantataulut tarvittaessa ja lisätään oletusdata
            Invoices = await Task.Run(() =>
            {
                repo.CreateInvoiceDb();
                repo.CreateCustomersTable();
                repo.CreateProductsTable();
                repo.CreateInvoicesTable();
                repo.CreateInvoiceRowsTable();
                repo.AddDefaultCustomers();
                repo.AddDefaultProducts();
                repo.AddDefaultInvoices();
                repo.AddDefaultInvoiceRows();

                // Palautetaan haetut laskut
                return repo.GetInvoices();
            });

            // Tulostetaan debug-lokitietoja haetuista laskuista ja niiden riveistä
            foreach (var invoice in Invoices)
            {
                Debug.WriteLine($"Invoice ID: {invoice.InvoiceId}, Rows count: {invoice.Rows.Count}");
                foreach (var row in invoice.Rows)
                {
                    Debug.WriteLine($"RowType: {row.RowType}, ProductName: {row.ProductName}, Description: {row.Description}");
                }
            }

            // Piilotetaan latausviesti ja näytetään pääsisältö
            LoadingMessage.Visibility = Visibility.Collapsed;
            MainContent.Visibility = Visibility.Visible;

            // Päivitetään pääsisältö
            UpdateMainContent();
        }

        // Päivitetään pääsisältö näyttämään valittu lasku
        public void UpdateMainContent()
        {
            if (Invoices.Count != 0)
            {
                // Varmistetaan, ettei valittu indeksi ylitä laskujen määrää
                if (selectedIndex >= Invoices.Count)
                {
                    selectedIndex = Invoices.Count - 1; // Siirrytään viimeiseen olemassa olevaan laskuun
                }

                // Luodaan uusi InvoiceView tai päivitetään olemassa oleva
                if (invoiceView == null)
                {
                    invoiceView = new InvoiceView(Invoices[selectedIndex]);
                }
                else
                {
                    invoiceView.DisplayedInvoice = Invoices[selectedIndex];
                }

                // Piilotetaan "Ei laskuja" -viesti ja näytetään pääsisältö
                NoInvoicesMessage.Visibility = Visibility.Collapsed;
                MainContent.Visibility = Visibility.Visible;

                // Asetetaan InvoiceView pääsisällöksi
                MainContent.Content = invoiceView;
            }
            else
            {
                MainContent.Visibility = Visibility.Collapsed;
                NoInvoicesMessage.Visibility = Visibility.Visible;
            }
        }

        // CRUD BUTTONS FOR INVOICES

        #region CRUD BUTTONS FOR INVOICES
        private void CreateNewInvoiceClicked(object sender, RoutedEventArgs e)
        {
            CreateNewInvoice createNewInvoice = new CreateNewInvoice();
            createNewInvoice.Show();
            openWindows.Add(createNewInvoice);
        }

        private void ShowAllInvoices(object sender, RoutedEventArgs e)
        {
            ShowAllInvoices showAllInvoices = new ShowAllInvoices(this);
            showAllInvoices.Show();
            openWindows.Add(showAllInvoices);
        }

        private void UpdateCurrentInvoice(object sender, RoutedEventArgs e)
        {
            UpdateInvoice updateInvoice = new UpdateInvoice(selectedIndex);
            updateInvoice.Show();
            openWindows.Add(updateInvoice);
        }

        private void UpdateInvoiceClicked(object sender, RoutedEventArgs e)
        {
            UpdateInvoice updateInvoice = new UpdateInvoice();
            updateInvoice.Show();
            openWindows.Add(updateInvoice);
        }

        private void DeleteCurrentInvoice(object sender, RoutedEventArgs e)
        {
            DeleteInvoice deleteInvoice = new DeleteInvoice(selectedIndex);
            deleteInvoice.Show();
            openWindows.Add(deleteInvoice);
        }

        private void DeleteInvoiceClicked(object sender, RoutedEventArgs e)
        {
            DeleteInvoice deleteInvoice = new DeleteInvoice();
            deleteInvoice.Show();
            openWindows.Add(deleteInvoice);
        }

        #endregion

        // CRUD BUTTONS FOR CUSTOMERS
        #region CRUD BUTTONS FOR CUSTOMERS
        private void CreateNewCustomer(object sender, RoutedEventArgs e)
        {
            CreateNewCustomer createNewCustomer = new CreateNewCustomer();
            createNewCustomer.Show();
            openWindows.Add(createNewCustomer);
        }

        private void ShowAllCustomers(object sender, RoutedEventArgs e)
        {
            ShowAllCustomers showAllCustomers = new ShowAllCustomers();
            showAllCustomers.Show();
            openWindows.Add(showAllCustomers);
        }

        private void UpdateCustomerClicked(object sender, RoutedEventArgs e)
        {
            UpdateCustomer updateCustomer = new UpdateCustomer();
            updateCustomer.Show();
            openWindows.Add(updateCustomer);
        }
        private void DeleteCustomerClicked(object sender, RoutedEventArgs e)
        {
            DeleteCustomer deleteCustomer = new DeleteCustomer();
            deleteCustomer.Show();
            openWindows.Add(deleteCustomer);
        }

        #endregion

        //CRUD BUTTONS FOR PRODUCTS
        #region CRUD BUTTONS FOR PRODUCTS
        private void CreateNewProduct(object sender, RoutedEventArgs e)
        {
            CreateNewProduct createNewProduct = new CreateNewProduct();
            createNewProduct.Show();
            openWindows.Add(createNewProduct);
        }

        private void ShowAllProducts(object sender, RoutedEventArgs e)
        {
            ShowAllProducts showAllProducts = new ShowAllProducts();
            showAllProducts.Show();
            openWindows.Add(showAllProducts);
        }

        private void UpdateProduct(object sender, RoutedEventArgs e)
        {
            UpdateProduct updateProduct = new UpdateProduct();
            updateProduct.Show();
            openWindows.Add(updateProduct);
        }
        private void DeleteProduct(object sender, RoutedEventArgs e)
        {
            DeleteProduct deleteProduct = new DeleteProduct();
            deleteProduct.Show();
            openWindows.Add(deleteProduct);
        }

        #endregion

        // ARROW BUTTONS
        #region ARROW BUTTONS
        private void FirstClicked(object sender, RoutedEventArgs e)
        {
            selectedIndex = 0;
            UpdateMainContent();
        }

        private void PreviousClicked(object sender, RoutedEventArgs e)
        {
            if (selectedIndex > 0)
            {
                selectedIndex--;
                UpdateMainContent();
            }
        }

        private void NextClicked(object sender, RoutedEventArgs e)
        {
            if (selectedIndex < Invoices.Count - 1)
            {
                selectedIndex++;
                UpdateMainContent();
            }
        }

        private void LastClicked(object sender, RoutedEventArgs e)
        {
            if (Invoices.Count > 0) 
            {
                selectedIndex = Invoices.Count - 1; 
                UpdateMainContent();
            }
        }
        #endregion

        private void VersionClicked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Invoicing Application v1.0 (c)", "Version information");
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        
    }
}