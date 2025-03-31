using InvoicingApp.Models;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Reflection.PortableExecutable;

namespace InvoicingApp
{
    public class InvoiceRepository
    {
        // Connection string
        private const string local = @"Server=127.0.0.1; Port=3306; User ID=opiskelija; Pwd=opiskelija1;";
        private const string localWithDb = @"Server=127.0.0.1; Port=3306; User ID=opiskelija; Pwd=opiskelija1; Database=InvoiceDb;";

        public void CreateInvoiceDb()
        {
            using (MySqlConnection conn = new MySqlConnection(local))
            {
                try
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand("DROP DATABASE IF EXISTS InvoiceDb", conn);
                    cmd.ExecuteNonQuery();

                    cmd = new MySqlCommand("CREATE DATABASE InvoiceDb", conn);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while creating a database: " + ex.Message);
                }
            }
        }

        public int GetInvoiceCountByCustomer(int customerId)
        {
            using MySqlConnection conn = new MySqlConnection(localWithDb);
            conn.Open();

            string query = "SELECT COUNT(*) FROM Invoices WHERE CustomerID = @CustomerID";

            using MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@CustomerID", customerId);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // CREATE TABLES

        #region CREATE TABLES
        public void CreateCustomersTable()
        {
            using (MySqlConnection conn = new MySqlConnection(localWithDb))
            {
                conn.Open();

                string createTable = "CREATE TABLE Customers " +
                                     "(CustomerID INT NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                                     "Name VARCHAR(50) NOT NULL, " +
                                     "StreetAddress VARCHAR(100) NULL DEFAULT '', " +
                                     "PostalCode VARCHAR(10) NULL DEFAULT '', " +
                                     "City VARCHAR(50) NULL DEFAULT '');";

                MySqlCommand cmd = new MySqlCommand(createTable, conn);
                cmd.ExecuteNonQuery();
            }
        }

        public void CreateProductsTable()
        {
            using (MySqlConnection conn = new MySqlConnection(localWithDb))
            {
                conn.Open();

                string createTable = "CREATE TABLE Products " +
                                     "(ProductID INT NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                                     "Name VARCHAR(50) NOT NULL DEFAULT '', " +
                                     "Price DECIMAL(10,2) NOT NULL DEFAULT 0.00, " +
                                     "Description VARCHAR(200) NULL DEFAULT '');";

                MySqlCommand cmd = new MySqlCommand(createTable, conn);
                cmd.ExecuteNonQuery();
            }
        }

        public void CreateInvoicesTable()
        {
            using (MySqlConnection conn = new MySqlConnection(localWithDb))
            {
                conn.Open();

                string createTable = "CREATE TABLE Invoices " +
                                     "(InvoiceID INT NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                                     "CustomerID INT NOT NULL, " +
                                     "InvoiceDate DATE NOT NULL, " +
                                     "DueDate DATE NOT NULL, " +
                                     "Notes VARCHAR(500), " +
                                     "FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID) ON DELETE CASCADE);";

                MySqlCommand cmd = new MySqlCommand(createTable, conn);
                cmd.ExecuteNonQuery();
            }
        }

        public void CreateInvoiceRowsTable()
        {
            using (MySqlConnection conn = new MySqlConnection(localWithDb))
            {
                conn.Open();

                string createTable = @"CREATE TABLE InvoiceRows (
                                       InvoiceRowID INT NOT NULL AUTO_INCREMENT PRIMARY KEY, 
                                       InvoiceID INT NOT NULL, 
                                       RowType ENUM('Product', 'Work') NOT NULL,
                                       ProductID INT NULL, 
                                       ProductName VARCHAR(50) NULL DEFAULT '',
                                       UnitPrice DECIMAL(10,2) NULL, 
                                       Quantity INT NULL DEFAULT 0, 
                                       HourCost DECIMAL(10,2) NULL DEFAULT 0, 
                                       WorkHours DECIMAL(10,2) NULL DEFAULT 0, 
                                       Description VARCHAR(500) NULL DEFAULT '',
                                       FOREIGN KEY (InvoiceID) REFERENCES Invoices(InvoiceID) ON DELETE CASCADE, 
                                       FOREIGN KEY (ProductID) REFERENCES Products(ProductID) ON DELETE SET NULL);";

                MySqlCommand cmd = new MySqlCommand(createTable, conn);
                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        // ADD DEFAULTS

        #region ADD DEFAULTS
        public void AddDefaultCustomers()
        {
            using MySqlConnection conn = new MySqlConnection(localWithDb);
            conn.Open();
            MySqlTransaction tr = conn.BeginTransaction();

            try
            {
                // SQL-query
                string invoice1 = "INSERT INTO Customers(Name, StreetAddress, PostalCode, City) VALUES('Hemmo P.', 'Kämmekkätie 6', '80130', 'JOENSUU')";
                string invoice2 = "INSERT INTO Customers(Name, StreetAddress, PostalCode, City) VALUES('Oskari Rautiainen', 'Joensuuntie 516', '82350', 'TOHMAJÄRVI')";
                string invoice3 = "INSERT INTO Customers(Name, StreetAddress, PostalCode, City) VALUES('Eetu R.', null, '80170', 'JOENSUU')";
                string invoice4 = "INSERT INTO Customers(Name, StreetAddress, PostalCode, City) VALUES('Markku S.', 'Paiholantie 52', '80850', 'KONTIOLAHTI')";

                // Create command and set transaction
                MySqlCommand cmd = new MySqlCommand(invoice1, conn, tr);
                cmd.ExecuteNonQuery();

                cmd = new MySqlCommand(invoice2, conn, tr);
                cmd.ExecuteNonQuery();

                cmd = new MySqlCommand(invoice3, conn, tr);
                cmd.ExecuteNonQuery();

                cmd = new MySqlCommand(invoice4, conn, tr);
                cmd.ExecuteNonQuery();

                // Commit transaction
                tr.Commit();
            }
            catch (Exception ex)
            {
                // Rollback transaction 
                tr.Rollback();
                Debug.WriteLine("Transaction cancelled. Error: " + ex.Message);
                throw new InvalidOperationException("An error occurred while processing the transaction.", ex);
            }
            finally
            {
                // Close connection
                conn.Close();
            }

        }

        public void AddDefaultProducts()
        {
            using MySqlConnection conn = new MySqlConnection(localWithDb);
            conn.Open();

            MySqlTransaction tr = conn.BeginTransaction();

            try
            {
                // SQL-query
                string product1 = "INSERT INTO Products(Name, Price) VALUES('Drywall', 16.50)";
                string product2 = "INSERT INTO Products(Name, Price, Description) VALUES('Drywall Joint Tape', 11.00, '50m')";
                string product3 = "INSERT INTO Products(Name, Price, Description) VALUES('Wall Filler', 43.95, '10l gray')";
                string product4 = "INSERT INTO Products(Name, Price, Description) VALUES('Deck Board', 1.74, 'Price/metre 28x95 mm')";
                string product5 = "INSERT INTO Products(Name, Price, Description) VALUES('Deck Screw ', 7.99, '5,0 x 70 mm A2 RST 100pc')";
                string product6 = "INSERT INTO Products(Name, Price, Description) VALUES('Polycarbonate Roof Sheet', 86.00, '0,8x1040x4000 mm Clear')";
                string product7 = "INSERT INTO Products(Name, Price, Description) VALUES('Cement Bag', 7.60, '25kg')";

                // Create command and set 
                MySqlCommand cmd = new MySqlCommand(product1, conn, tr);
                cmd.ExecuteNonQuery();

                cmd = new MySqlCommand(product2, conn, tr);
                cmd.ExecuteNonQuery();

                cmd = new MySqlCommand(product3, conn, tr);
                cmd.ExecuteNonQuery();

                cmd = new MySqlCommand(product4, conn, tr);
                cmd.ExecuteNonQuery();

                cmd = new MySqlCommand(product5, conn, tr);
                cmd.ExecuteNonQuery();

                cmd = new MySqlCommand(product6, conn, tr);
                cmd.ExecuteNonQuery();

                cmd = new MySqlCommand(product7, conn, tr);
                cmd.ExecuteNonQuery();

                // Commit transaction
                tr.Commit();
            }
            catch (Exception ex)
            {
                // Rollback transaction 
                tr.Rollback();
                Debug.WriteLine("Transaction cancelled. Error: " + ex.Message);
                throw new InvalidOperationException("An error occurred while processing the transaction.", ex);

            }
            finally
            {
                // Close connection
                conn.Close();
            }
        }

        public void AddDefaultInvoiceRows()
        {
            using MySqlConnection conn = new MySqlConnection(localWithDb);
            conn.Open();
            MySqlTransaction tr = conn.BeginTransaction();

            try
            {

                // Lisää laskurivit yhdellä INSERT-kyselyllä
                string rowsQuery = @"INSERT INTO InvoiceRows (InvoiceID, ProductID, ProductName, UnitPrice, Quantity, HourCost, WorkHours, Description, RowType)
                                     SELECT 2, ProductID, Name, Price, 15, 13, NULL, '120cm x 60cm', 'Product'
                                     FROM Products WHERE ProductID = 1;

                                     INSERT INTO InvoiceRows (InvoiceID, HourCost, WorkHours, Description, RowType)
                                     VALUES (2, 13, 16, 'Drywall Installation', 'Work');

                                     INSERT INTO InvoiceRows (InvoiceID, ProductID, ProductName, UnitPrice, Quantity, HourCost, WorkHours, Description, RowType)
                                     SELECT 2, ProductID, Name, Price, 1, NULL, NULL, Description, 'Product'
                                     FROM Products WHERE ProductID = 2;

                                     INSERT INTO InvoiceRows (InvoiceID, ProductID, ProductName, UnitPrice, Quantity, HourCost, WorkHours, Description, RowType)
                                     SELECT 4, ProductID, Name, Price, 128, NULL, NULL, Description, 'Product'
                                     FROM Products WHERE ProductID = 4;

                                     INSERT INTO InvoiceRows (InvoiceID, ProductID, ProductName, UnitPrice, Quantity, HourCost, WorkHours, Description, RowType)
                                     SELECT 4, ProductID, Name, Price, 7, NULL, NULL, Description, 'Product'
                                     FROM Products WHERE ProductID = 5;

                                     INSERT INTO InvoiceRows (InvoiceID, ProductID, ProductName, UnitPrice, Quantity, HourCost, WorkHours, Description, RowType)
                                     SELECT 4, ProductID, Name, Price, 4, NULL, NULL, Description, 'Product'
                                     FROM Products WHERE ProductID = 6;

                                     INSERT INTO InvoiceRows (InvoiceID, ProductID, ProductName, UnitPrice, Quantity, HourCost, WorkHours, Description, RowType)
                                     SELECT 5, ProductID, Name, Price, 4, NULL, NULL, Description, 'Product'
                                     FROM Products WHERE ProductID = 7;

                                     INSERT INTO InvoiceRows (InvoiceID, ProductID, ProductName, UnitPrice, Quantity, HourCost, WorkHours, Description, RowType) 
                                     VALUES 
                                     (1, NULL, NULL, NULL, NULL, 15.00, 2, 'Cleaning the roof in autumn.', 'Work'),

                                     (3, NULL, NULL, NULL, NULL, 13.00, 9, 'Firewood chopping.', 'Work'),

                                     (4, NULL, NULL, NULL, NULL, 40, 15, 'Terassin laudoitus ja valokatteen asennus', 'Work'),

                                     (4, NULL, NULL, NULL, NULL, 40, -3, 'Kanta-asiakas alennus', 'Work'),

                                     (5, NULL, NULL, NULL, NULL, 13.50, 3, 'Kylpyhuoneen lattiavalu', 'Work');";


                MySqlCommand cmd = new MySqlCommand(rowsQuery, conn, tr);
                cmd.ExecuteNonQuery();

                tr.Commit();
            }
            catch (Exception ex)
            {
                tr.Rollback();
                Debug.WriteLine("Error adding invoice rows: " + ex.Message);
                throw new InvalidOperationException("An error occurred while processing the transaction.", ex);
            }
            finally
            {
                // Close connection
                conn.Close();
            }
        }

        public void AddDefaultInvoices()
        {
            using MySqlConnection conn = new MySqlConnection(localWithDb);
            conn.Open();
            MySqlTransaction tr = conn.BeginTransaction();

            try
            {
                string invoicesQuery = @"INSERT INTO Invoices (CustomerID, InvoiceDate, DueDate, Notes) 
                                             VALUES 
                                             (1, '2024-09-17', '2024-10-01', 'First invoice'),
                                             (2, '2024-11-20', '2024-12-04', NULL),
                                             (3, '2025-03-06', '2025-03-20', 'Maksaa, kun jaksaa.'),
                                             (4, '2025-03-23', '2025-04-01', 'Terassitarvikkeet'),
                                             (2, '2024-12-15', '2024-12-29', 'Kylpyhuoneen lattiavalu')";

                MySqlCommand cmd = new MySqlCommand(invoicesQuery, conn, tr);
                cmd.ExecuteNonQuery();

                tr.Commit();
            }
            catch (Exception ex)
            {
                tr.Rollback();
                Console.WriteLine("Error adding invoices: " + ex.Message);
                throw new InvalidOperationException("An error occurred while processing the transaction.", ex);
            }
            finally
            {
                // Close connection
                conn.Close();
            }
        }

        #endregion


        // GETS

        #region GET METHODS

        public ObservableCollection<Invoice> GetInvoices()
        {
            ObservableCollection<Invoice> invoices = new();

            using (MySqlConnection conn = new MySqlConnection(localWithDb))
            {
                conn.Open();

                string query = @"SELECT i.InvoiceID, i.CustomerID, i.InvoiceDate, i.DueDate, i.Notes, 
                                        c.Name, c.StreetAddress, c.PostalCode, c.City
                                 FROM Invoices i
                                 JOIN Customers c ON i.CustomerID = c.CustomerID";

                MySqlCommand cmd = new(query, conn);
                MySqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    Invoice invoice = new Invoice
                    {
                        InvoiceId = dr.GetInt32("InvoiceID"),
                        InvoiceDate = dr.GetDateTime("InvoiceDate"),
                        DueDate = dr.GetDateTime("DueDate"),
                        Notes = dr["Notes"] as string ?? "",
                        Customer = new Customer
                        {
                            CustomerId = dr.GetInt32("CustomerID"),
                            Name = dr.GetString("Name"),
                            StreetAddress = dr["StreetAddress"] as string ?? "",
                            PostalCode = dr["PostalCode"] as string ?? "",
                            City = dr["City"] as string ?? ""
                        }
                    };

                    // Haetaan laskurivit ja lisätään ne laskuun
                    invoice.Rows = GetInvoiceRows(invoice.InvoiceId);


                    //Debug.WriteLine($"Haettu lasku ID: {invoice.InvoiceId}, rivien määrä: {invoice.Rows.Count}");
                    invoices.Add(invoice);
                }

            }

            return invoices;
        }

        public ObservableCollection<InvoiceRow> GetInvoiceRows(int? invoiceId)
        {
            ObservableCollection<InvoiceRow> rows = new();

            // Jos invoiceId on null, ei kannata suorittaa SQL-kyselyä
            if (!invoiceId.HasValue)
            {
                Debug.WriteLine("GetInvoiceRows: InvoiceId is null, returning empty list.");
                return rows;
            }


            using (MySqlConnection conn = new MySqlConnection(localWithDb))
            {
                conn.Open();

                string query = @"SELECT InvoiceRowID, ProductID, RowType, ProductName, Quantity, UnitPrice, WorkHours, HourCost, Description 
                                 FROM InvoiceRows 
                                 WHERE InvoiceID = @InvoiceID";

                MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
                MySqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    InvoiceRow row = new InvoiceRow
                    {
                        InvoiceRowId = dr.GetInt32("InvoiceRowID"),
                        ProductId = dr.IsDBNull(dr.GetOrdinal("ProductID")) ? null : dr.GetInt32("ProductID"),
                        RowType = dr["RowType"] as string ?? "",
                        ProductName = dr["ProductName"] as string ?? "",
                        Quantity = dr.IsDBNull(dr.GetOrdinal("Quantity")) ? 0 : dr.GetInt32("Quantity"),
                        UnitPrice = dr.IsDBNull(dr.GetOrdinal("UnitPrice")) ? 0m : dr.GetDecimal("UnitPrice"),
                        WorkHours = dr.IsDBNull(dr.GetOrdinal("WorkHours")) ? 0m : dr.GetDecimal("WorkHours"),
                        HourCost = dr.IsDBNull(dr.GetOrdinal("HourCost")) ? 0m : dr.GetDecimal("HourCost"),
                        Description = dr["Description"] as string ?? ""
                    };

                    rows.Add(row);

                    Debug.WriteLine($"Row: ProductId={row.ProductId}, ProductName={row.ProductName}");
                }
            }

            //Debug.WriteLine($"Total rows fetched for invoice {invoiceId}: {rows.Count}");
            return rows;
        }



        public ObservableCollection<Customer> GetCustomers()
        {
            ObservableCollection<Customer> customers = new ObservableCollection<Customer>();


            using (MySqlConnection conn = new MySqlConnection(localWithDb))
            {
                conn.Open();

                MySqlCommand cmd = new("SELECT * FROM customers", conn);
                MySqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {

                    Debug.WriteLine($"{dr["CustomerID"]} {dr["Name"]}");

                    Customer customer = new Customer
                    {
                        CustomerId = dr.GetInt32("CustomerID"),
                        Name = dr.GetString("Name"),
                        StreetAddress = dr["StreetAddress"] as string ?? "",
                        PostalCode = dr["PostalCode"] as string ?? "",
                        City = dr["City"] as string ?? ""
                    };

                    customers.Add(customer);
                }


            }

            return customers;
        }

        public ObservableCollection<Product> GetProducts()
        {
            ObservableCollection<Product> products = new ObservableCollection<Product>();


            using (MySqlConnection conn = new MySqlConnection(localWithDb))
            {
                conn.Open();

                MySqlCommand cmd = new("SELECT * FROM products", conn);
                MySqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {

                    Debug.WriteLine($"{dr["ProductID"]} {dr["Name"]}");

                    Product product = new Product
                    {
                        ProductId = dr.GetInt32("ProductID"),
                        Name = dr.GetString("Name"),
                        Price = dr.GetDecimal("Price"),
                        Description = dr["Description"] as string ?? "",
                    };

                    products.Add(product);
                }


            }

            return products;
        }

        #endregion


        // CREATES

        #region CREATE METHODS


        public bool CreateInvoice(Invoice invoice)
        {
            using MySqlConnection conn = new MySqlConnection(localWithDb);
            conn.Open();
            MySqlTransaction tr = conn.BeginTransaction();

            try
            {
                // Luodaan uusi lasku ja haetaan ID heti
                string insertInvoiceQuery = @"INSERT INTO Invoices (CustomerID, InvoiceDate, DueDate, Notes) 
                                     VALUES (@CustomerID, @InvoiceDate, @DueDate, @Notes);
                                     SELECT LAST_INSERT_ID();";

                using (MySqlCommand cmd = new MySqlCommand(insertInvoiceQuery, conn, tr))
                {
                    cmd.Parameters.AddWithValue("@CustomerID", invoice.Customer.CustomerId);
                    cmd.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
                    cmd.Parameters.AddWithValue("@DueDate", invoice.DueDate);
                    cmd.Parameters.AddWithValue("@Notes", invoice.Notes);

                    // Haetaan lisätyn laskun ID
                    invoice.InvoiceId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Lisätään laskurivit
                foreach (var row in invoice.Rows)
                {
                    CreateInvoiceRow(row, invoice.InvoiceId, conn, tr);
                }

                tr.Commit();
                return true;
            }
            catch (Exception ex)
            {
                tr.Rollback();
                Debug.WriteLine("Error creating invoice: " + ex.Message);
                return false;
            }
            finally
            {
                conn.Close();
            }
        }

        public bool CreateCustomer(Customer customer)
        {
            using MySqlConnection conn = new MySqlConnection(localWithDb);
            conn.Open();
            MySqlTransaction tr = conn.BeginTransaction();

            try
            {

                string insertCustomerQuery = @"INSERT INTO Customers(Name, StreetAddress, PostalCode, City) 
                                                      VALUES(@Name, @StreetAddress, @PostalCode, @City); 
                                                      SELECT LAST_INSERT_ID();";

                using (MySqlCommand cmd = new MySqlCommand(insertCustomerQuery, conn, tr))
                {
                    cmd.Parameters.AddWithValue("@Name", customer.Name);
                    cmd.Parameters.AddWithValue("@StreetAddress", customer.StreetAddress);
                    cmd.Parameters.AddWithValue("@PostalCode", customer.PostalCode);
                    cmd.Parameters.AddWithValue("@City", customer.City);
                    
                    customer.CustomerId = Convert.ToInt32(cmd.ExecuteScalar());
                }
                tr.Commit();

                return true;
            }
            catch (Exception ex)
            {
                tr.Rollback();
                Debug.WriteLine("Error adding new customer: " + ex.Message);
                return false;
            }
            finally
            {
                // Close connection
                conn.Close();
            }
        }

        public bool CreateProduct(Product product)
        {
            using MySqlConnection conn = new MySqlConnection(localWithDb);
            conn.Open();
            MySqlTransaction tr = conn.BeginTransaction();

            try
            {
                MySqlCommand cmd = new MySqlCommand("INSERT INTO Products(Name, Price, Description) VALUES(@Name, @Price, @Description)", conn, tr);
                cmd.Parameters.AddWithValue("@Name", product.Name);
                cmd.Parameters.AddWithValue("@Price", product.Price);
                cmd.Parameters.AddWithValue("@Description", product.Description);
                int rowsAffected = cmd.ExecuteNonQuery();

                tr.Commit();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                tr.Rollback();
                Debug.WriteLine("Error adding new product: " + ex.Message);
                return false;
            }
            finally
            {
                // Close connection
                conn.Close();
            }
        }

       

        public void CreateInvoiceRow(InvoiceRow row, int? invoiceId, MySqlConnection conn, MySqlTransaction tr)
        {
            if (!invoiceId.HasValue)
            {
                Debug.WriteLine("Error: InvoiceId is null, cannot create invoice row.");
                return;
            }

            string query = @"INSERT INTO InvoiceRows (InvoiceID, ProductID, ProductName, Quantity, UnitPrice, Description, RowType, HourCost, WorkHours) 
                     VALUES (@InvoiceID, @ProductID, @ProductName, @Quantity, @UnitPrice, @Description, @RowType, @HourCost, @WorkHours)";

            using MySqlCommand cmd = new MySqlCommand(query, conn, tr);
            cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
            cmd.Parameters.AddWithValue("@ProductID", row.ProductId);
            cmd.Parameters.AddWithValue("@ProductName", row.ProductName);
            cmd.Parameters.AddWithValue("@Quantity", row.Quantity);
            cmd.Parameters.AddWithValue("@UnitPrice", row.UnitPrice);
            cmd.Parameters.AddWithValue("@Description", row.Description);
            cmd.Parameters.AddWithValue("@RowType", row.RowType);
            cmd.Parameters.AddWithValue("@HourCost", row.HourCost);
            cmd.Parameters.AddWithValue("@WorkHours", row.WorkHours);
            cmd.ExecuteNonQuery();
        }

        #endregion

        // DELETES

        #region DELETE METHODS

        public bool DeleteProduct(int productId)
        {
            using MySqlConnection conn = new MySqlConnection(localWithDb);
            conn.Open();
            MySqlTransaction tr = conn.BeginTransaction();

            try
            {
                MySqlCommand cmd = new MySqlCommand("DELETE FROM Products WHERE ProductId = @ProductId", conn, tr);
                cmd.Parameters.AddWithValue("@ProductId", productId);
                int rowsAffected = cmd.ExecuteNonQuery();

                tr.Commit();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                tr.Rollback();
                Debug.WriteLine("Error deleting product: " + ex.Message);
                return false;
            }
            finally
            {
                conn.Close();
            }
        }

        public bool DeleteCustomer(int customerId)
        {
            using MySqlConnection conn = new MySqlConnection(localWithDb);
            conn.Open();

            string deleteCustomerQuery = "DELETE FROM Customers WHERE CustomerID = @CustomerID";

            using MySqlCommand cmd = new MySqlCommand(deleteCustomerQuery, conn);
            cmd.Parameters.AddWithValue("@CustomerID", customerId);

            return cmd.ExecuteNonQuery() > 0;
        }

        public void DeleteInvoiceRow(int invoiceRowId, MySqlConnection conn, MySqlTransaction tr)
        {
            string deleteQuery = "DELETE FROM InvoiceRows WHERE InvoiceRowID = @InvoiceRowID";
            using MySqlCommand cmd = new MySqlCommand(deleteQuery, conn, tr);
            cmd.Parameters.AddWithValue("@InvoiceRowID", invoiceRowId);
            cmd.ExecuteNonQuery();
        }

        public bool DeleteInvoice(int invoiceId)
        {
            using MySqlConnection conn = new MySqlConnection(localWithDb);
            conn.Open();
            MySqlTransaction tr = conn.BeginTransaction();

            try
            {
                MySqlCommand cmd = new MySqlCommand("DELETE FROM Invoices WHERE InvoiceId = @InvoiceId", conn, tr);
                cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                int rowsAffected = cmd.ExecuteNonQuery();

                tr.Commit();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                tr.Rollback();
                Debug.WriteLine("Error deleting invoice: " + ex.Message);
                return false;
            }
            finally
            {
                conn.Close();
            }
        }



        #endregion

        // UPDATES

        #region UPDATE METHODS

        public bool UpdateProduct(Product product)
        {
            //Debug.WriteLine($"Updating Product: ID={product?.ProductId}, Name={product?.Name}, Price={product?.Price}, Desc={product?.Description}");

            using MySqlConnection conn = new MySqlConnection(localWithDb);

            conn.Open();
            MySqlTransaction tr = conn.BeginTransaction();

            try
            {
                string updateQuery = @"UPDATE Products 
                                       SET Name = @Name, 
                                           Price = @Price, 
                                           Description = @Description 
                                       WHERE ProductId = @ProductId";

                MySqlCommand cmd = new MySqlCommand(updateQuery, conn, tr);
                cmd.Parameters.AddWithValue("@Name", product.Name);
                cmd.Parameters.AddWithValue("@Price", product.Price);
                cmd.Parameters.AddWithValue("@Description", product.Description);
                cmd.Parameters.AddWithValue("@ProductId", product.ProductId);

                int rowsAffected = cmd.ExecuteNonQuery();
                tr.Commit();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                tr.Rollback();
                Debug.WriteLine("Error updating product: " + ex.ToString());
                return false;
            }
            finally
            {
                conn.Close();
            }
        }

        public bool UpdateCustomer(Customer customer)
        {
            using MySqlConnection conn = new MySqlConnection(localWithDb);
            conn.Open();
            MySqlTransaction tr = conn.BeginTransaction();

            try
            {
                string query = @"UPDATE Customers 
                             SET Name = @Name, 
                                 StreetAddress = @StreetAddress, 
                                 PostalCode = @PostalCode, 
                                 City = @City 
                             WHERE CustomerID = @CustomerID";

                using MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CustomerID", customer.CustomerId);
                cmd.Parameters.AddWithValue("@Name", customer.Name);
                cmd.Parameters.AddWithValue("@StreetAddress", customer.StreetAddress);
                cmd.Parameters.AddWithValue("@PostalCode", customer.PostalCode);
                cmd.Parameters.AddWithValue("@City", customer.City);

                int rowsAffected = cmd.ExecuteNonQuery();
                tr.Commit();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                tr.Rollback();
                Debug.WriteLine("Error updating customer: " + ex.ToString());
                return false;
            }
            finally
            {
                conn.Close();
            }

        }

        public void UpdateInvoiceRow(InvoiceRow row, MySqlConnection conn, MySqlTransaction tr)
        {
            string updateInvoiceRowQuery = @"UPDATE InvoiceRows 
                                             SET ProductID = @ProductID,
                                                 ProductName = @ProductName,
                                                 Quantity = @Quantity, 
                                                 UnitPrice = @UnitPrice, 
                                                 Description = @Description, 
                                                 RowType = @RowType
                                             WHERE InvoiceRowID = @InvoiceRowID";

            using MySqlCommand cmd = new MySqlCommand(updateInvoiceRowQuery, conn, tr);
            cmd.Parameters.AddWithValue("@InvoiceRowID", row.InvoiceRowId);
            cmd.Parameters.AddWithValue("@ProductID", row.ProductId);
            cmd.Parameters.AddWithValue("@ProductName", row.ProductName);
            cmd.Parameters.AddWithValue("@Quantity", row.Quantity);
            cmd.Parameters.AddWithValue("@UnitPrice", row.UnitPrice);
            cmd.Parameters.AddWithValue("@Description", row.Description);
            cmd.Parameters.AddWithValue("@RowType", row.RowType);
            cmd.ExecuteNonQuery();
        }

        public bool UpdateInvoice(Invoice invoice)
        {
            // Tarkistetaan, onko InvoiceId null
            if (!invoice.InvoiceId.HasValue)
            {
                Debug.WriteLine("UpdateInvoice: InvoiceId is null, cannot update invoice.");
                return false;
            }

            using MySqlConnection conn = new MySqlConnection(localWithDb);
            conn.Open();
            MySqlTransaction tr = conn.BeginTransaction();

            try
            {
                // Päivitetään laskun perustiedot (POISTETTU CustomerName)
                string updateInvoiceQuery = @"UPDATE Invoices 
                                              SET InvoiceDate = @InvoiceDate,
                                                  DueDate = @DueDate,
                                                  Notes = @Notes
                                              WHERE InvoiceID = @InvoiceID";

                MySqlCommand cmd = new MySqlCommand(updateInvoiceQuery, conn, tr);
                cmd.Parameters.AddWithValue("@InvoiceID", invoice.InvoiceId);
                cmd.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
                cmd.Parameters.AddWithValue("@DueDate", invoice.DueDate);
                cmd.Parameters.AddWithValue("@Notes", invoice.Notes);
                cmd.ExecuteNonQuery();

                // Haetaan nykyiset rivien ID:t
                HashSet<int> existingRowIds = new HashSet<int>();

                MySqlCommand Cmd = new MySqlCommand("SELECT InvoiceRowID FROM InvoiceRows WHERE InvoiceID = @InvoiceID", conn, tr);
                Cmd.Parameters.AddWithValue("@InvoiceID", invoice.InvoiceId);

                using (MySqlDataReader rd = Cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        existingRowIds.Add(rd.GetInt32("InvoiceRowID"));
                    }
                }

                // Käydään kaikki rivit läpi
                foreach (InvoiceRow row in invoice.Rows)
                {
                    if (row.InvoiceRowId.HasValue && existingRowIds.Contains(row.InvoiceRowId.Value))
                    {
                        UpdateInvoiceRow(row, conn, tr);
                    }
                    else
                    {
                        CreateInvoiceRow(row, invoice.InvoiceId, conn, tr);
                    }
                }

                /* // Poistetaan rivit, joita ei enää ole
                var rowIdsToDelete = existingRowIds.Except(invoice.Rows.Where(r => r.InvoiceRowId.HasValue).Select(r => r.InvoiceRowId!.Value));
                foreach (var rowId in rowIdsToDelete)
                {
                    DeleteInvoiceRow(rowId, conn, tr);
                }*/

                // Haetaan nykyiset rivien ID:t
                List<int> currentRowIds = new List<int>();
                foreach (var row in invoice.Rows)
                {
                    if (row.InvoiceRowId.HasValue)
                    {
                        currentRowIds.Add(row.InvoiceRowId.Value);
                    }
                }

                // Poistetaan rivit, joita ei enää ole
                foreach (var rowId in existingRowIds)
                {
                    if (!currentRowIds.Contains(rowId))
                    {
                        DeleteInvoiceRow(rowId, conn, tr);
                    }
                }

                tr.Commit();
                return true;
            }
            catch (Exception ex)
            {
                tr.Rollback();
                Debug.WriteLine("Error updating invoice: " + ex.Message);
                return false;
            }
            finally
            {
                conn.Close();
            }
        }

        #endregion

    }
}