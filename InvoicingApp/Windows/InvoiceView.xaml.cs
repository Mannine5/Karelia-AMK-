using InvoicingApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    /// Interaction logic for InvoiceView.xaml
    /// </summary>
    public partial class InvoiceView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        public ObservableCollection<InvoiceRow> ProductRows { get; } = new();
        public ObservableCollection<InvoiceRow> WorkRows { get; } = new();

        public InvoiceView(Invoice displayedInvoice)
        {
            InitializeComponent();
            DataContext = this;
            DisplayedInvoice = displayedInvoice;
        }

        public void UpdateFilteredLists()
        {
            ProductRows.Clear();
            WorkRows.Clear();

            if (DisplayedInvoice?.Rows == null) return;

            foreach (InvoiceRow row in DisplayedInvoice.Rows)
            {
                (row.RowType == "Product" ? ProductRows : WorkRows).Add(row);
            }
        }

        private decimal CalculateTotalPrice()
        {
            return ProductRows.Concat(WorkRows)
                              .Sum(row => row.Quantity * row.UnitPrice + row.HourCost * row.WorkHours);
        }

        private void UpdateTotalPrice()
        {
            TotalPrice = CalculateTotalPrice();
            OnPropertyChanged(nameof(TotalPrice));
        }
    }
}
