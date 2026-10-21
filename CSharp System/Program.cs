/*
 * Smart Inventory Monitoring and Order Management System (Moon & Mug)
 * MIDTERM PERIOD - C# (.NET) Object-Oriented Programming Implementation
 * Matches the Use Class Diagram & Functional Analysis in the Project Documentation.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartInventoryAndOrderManagement
{
    // ==========================================
    // 1. USER, ADMINISTRATOR & STAFF (INHERITANCE & ENCAPSULATION)
    // ==========================================
    public class User
    {
        private int userID;
        private string username;
        private string password;
        private string fullName;
        private string role;
        private bool isLoggedIn;

        public int UserID { get => userID; set => userID = value; }
        public string Username { get => username; set => username = value; }
        public string FullName { get => fullName; set => fullName = value; }
        public string Role { get => role; set => role = value; }

        public User(int userID, string username, string password, string fullName, string role)
        {
            this.userID = userID;
            this.username = username;
            this.password = password;
            this.fullName = fullName;
            this.role = role;
            this.isLoggedIn = false;
        }

        public virtual bool login(string enteredUsername, string enteredPassword)
        {
            if (this.username == enteredUsername && this.password == enteredPassword)
            {
                this.isLoggedIn = true;
                Console.WriteLine($"[LOGIN SUCCESS] Welcome, {this.fullName} ({this.role})!");
                return true;
            }
            Console.WriteLine("[LOGIN ERROR] Invalid username or password.");
            return false;
        }

        public virtual void logout()
        {
            this.isLoggedIn = false;
            Console.WriteLine($"[LOGOUT] User '{this.username}' has logged out.");
        }
    }

    public class Administrator : User
    {
        public int adminID { get; set; }
        public string position { get; set; }

        public Administrator(int userID, string username, string password, string fullName, int adminID, string position)
            : base(userID, username, password, fullName, "Admin")
        {
            this.adminID = adminID;
            this.position = position;
        }

        public void manageUsers(List<Staff> staffList, Staff newStaff)
        {
            staffList.Add(newStaff);
            Console.WriteLine($"[ADMIN - STAFF MGMT] Added Staff: {newStaff.FullName} ({newStaff.position})");
        }

        public void manageProducts(List<Product> productCatalog, Product product, string action)
        {
            if (action == "add")
            {
                product.addProduct();
                productCatalog.Add(product);
            }
            else if (action == "update")
            {
                product.updateProduct();
            }
            else if (action == "delete")
            {
                product.deleteProduct();
                productCatalog.Remove(product);
            }
        }

        public void generateReport(Report report, List<Inventory> inventories, List<Order> orders)
        {
            Console.WriteLine($"\n[ADMIN - REPORT] {this.FullName} is generating report...");
            report.generateReport(inventories, orders);
            report.viewReport();
        }
    }

    public class Staff : User
    {
        public int staffID { get; set; }
        public string position { get; set; }

        public Staff(int userID, string username, string password, string fullName, int staffID, string position)
            : base(userID, username, password, fullName, "Staff")
        {
            this.staffID = staffID;
            this.position = position;
        }

        public void processOrder(Order order, List<Inventory> inventories)
        {
            Console.WriteLine($"\n[STAFF - POS] {this.FullName} processing Order #{order.orderID}...");
            order.createOrder();

            foreach (var detail in order.OrderDetails)
            {
                var inv = inventories.FirstOrDefault(i => i.productID == detail.productID);
                if (inv != null)
                {
                    inv.updateStock(-detail.quantity);
                }
            }

            order.updateOrder("Completed");
        }

        public void updateStock(Inventory inventory, int addedQty)
        {
            Console.WriteLine($"[STAFF - STOCK] {this.FullName} restocking Product #{inventory.productID}...");
            inventory.updateStock(addedQty);
        }

        public void monitorInventory(List<Inventory> inventories, List<Product> products)
        {
            Console.WriteLine($"\n--- INVENTORY MONITOR (Checked by {this.FullName}) ---");
            foreach (var inv in inventories)
            {
                var prod = products.FirstOrDefault(p => p.productID == inv.productID);
                string prodName = prod != null ? prod.productName : $"Product #{inv.productID}";
                string status = inv.checkLowStock() ? "LOW STOCK!" : "OK";
                Console.WriteLine($"InvID: {inv.inventoryID} | {prodName,-22} | Qty: {inv.checkStock(),-4} | Reorder Lvl: {inv.reorderLevel,-3} | Status: {status}");
            }
        }
    }

    // ==========================================
    // 2. PRODUCT & INVENTORY CLASSES
    // ==========================================
    public class Product
    {
        public int productID { get; set; }
        public string productName { get; set; }
        public string category { get; set; }
        public double price { get; set; }

        public Product(int productID, string productName, string category, double price)
        {
            this.productID = productID;
            this.productName = productName;
            this.category = category;
            this.price = price;
        }

        public void addProduct()
        {
            Console.WriteLine($"[PRODUCT ADDED] #{productID}: {productName} ({category}) - PHP {price:F2}");
        }

        public void updateProduct()
        {
            Console.WriteLine($"[PRODUCT UPDATED] #{productID}: {productName} - PHP {price:F2}");
        }

        public void deleteProduct()
        {
            Console.WriteLine($"[PRODUCT DELETED] #{productID}: {productName}");
        }
    }

    public class Inventory
    {
        public int inventoryID { get; set; }
        public int productID { get; set; }
        public int quantity { get; set; }
        public int reorderLevel { get; set; }

        public Inventory(int inventoryID, int productID, int quantity, int reorderLevel)
        {
            this.inventoryID = inventoryID;
            this.productID = productID;
            this.quantity = quantity;
            this.reorderLevel = reorderLevel;
        }

        public void updateStock(int change)
        {
            this.quantity += change;
            if (this.quantity < 0) this.quantity = 0;
            Console.WriteLine($"[INVENTORY #{inventoryID}] Product #{productID} stock is now {quantity} units.");
            if (checkLowStock())
            {
                Console.WriteLine($"  -> [ALERT] Low stock detected for Product #{productID} (Reorder Level: {reorderLevel})!");
            }
        }

        public int checkStock()
        {
            return this.quantity;
        }

        public bool checkLowStock()
        {
            return this.quantity <= this.reorderLevel;
        }
    }

    // ==========================================
    // 3. ORDER, ORDER DETAIL & REPORT CLASSES
    // ==========================================
    public class OrderDetail
    {
        public int orderDetailID { get; set; }
        public int orderID { get; set; }
        public int productID { get; set; }
        public int quantity { get; set; }
        public double unitPrice { get; set; }
        public double subtotal { get; set; }

        public OrderDetail(int orderDetailID, int orderID, Product product, int quantity, double extraCustomizationFee = 0.0)
        {
            this.orderDetailID = orderDetailID;
            this.orderID = orderID;
            this.productID = product.productID;
            this.quantity = quantity;
            this.unitPrice = product.price + extraCustomizationFee;
            this.subtotal = calculateTotal();
        }

        public double calculateTotal()
        {
            this.subtotal = this.unitPrice * this.quantity;
            return this.subtotal;
        }
    }

    public class Order
    {
        public int orderID { get; set; }
        public DateTime orderDate { get; set; }
        public double totalAmount { get; set; }
        public string status { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public Order(int orderID)
        {
            this.orderID = orderID;
            this.orderDate = DateTime.Now;
            this.status = "Pending";
            this.totalAmount = 0.0;
        }

        public void AddDetail(OrderDetail detail)
        {
            OrderDetails.Add(detail);
            totalAmount = OrderDetails.Sum(d => d.calculateTotal());
        }

        public void createOrder()
        {
            totalAmount = OrderDetails.Sum(d => d.calculateTotal());
            status = "Processing";
            Console.WriteLine($"[ORDER CREATED] Order #{orderID} ({orderDate:yyyy-MM-dd HH:mm}) | Total: PHP {totalAmount:F2}");
        }

        public void updateOrder(string newStatus)
        {
            this.status = newStatus;
            Console.WriteLine($"[ORDER UPDATED] Order #{orderID} status: {status}");
        }

        public void cancelOrder()
        {
            this.status = "Cancelled";
            Console.WriteLine($"[ORDER CANCELLED] Order #{orderID} has been cancelled.");
        }
    }

    public class Report
    {
        public int reportID { get; set; }
        public string reportType { get; set; }
        public DateTime dateGenerated { get; set; }
        private string reportSummary;

        public Report(int reportID, string reportType)
        {
            this.reportID = reportID;
            this.reportType = reportType;
            this.dateGenerated = DateTime.Now;
            this.reportSummary = "";
        }

        public void generateReport(List<Inventory> inventories, List<Order> orders)
        {
            var completed = orders.Where(o => o.status == "Completed").ToList();
            double totalRevenue = completed.Sum(o => o.totalAmount);
            int lowStockCount = inventories.Count(i => i.checkLowStock());

            reportSummary = "==========================================================\n" +
                            $" MOON & MUG - {reportType.ToUpper()}\n" +
                            $" Report ID: #{reportID} | Generated: {dateGenerated:yyyy-MM-dd HH:mm:ss}\n" +
                            "==========================================================\n" +
                            $" Total Completed Orders : {completed.Count}\n" +
                            $" Total Sales Revenue    : PHP {totalRevenue:F2}\n" +
                            $" Low-Stock Items Alert  : {lowStockCount}\n" +
                            "==========================================================";
        }

        public void viewReport()
        {
            Console.WriteLine(reportSummary);
        }
    }

    // ==========================================
    // MAIN PROGRAM ENTRY POINT
    // ==========================================
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== SMART INVENTORY MONITORING AND ORDER MANAGEMENT SYSTEM (C#) ===\n");

            Administrator admin = new Administrator(1, "admin", "1234", "Brics S. Coyoca", 101, "System Administrator");
            Staff staff = new Staff(2, "staff", "1234", "John Carlo Barbero", 201, "POS Cashier");
            List<Staff> staffAccounts = new List<Staff>();

            admin.login("admin", "1234");
            staff.login("staff", "1234");

            admin.manageUsers(staffAccounts, staff);

            List<Product> catalog = new List<Product>();
            Product p1 = new Product(1, "Classic Milk Tea", "Milk Tea", 90.00);
            Product p2 = new Product(2, "Okinawa Milk Tea", "Milk Tea", 110.00);
            Product p3 = new Product(3, "Coffee Frappe", "Frappe", 140.00);

            admin.manageProducts(catalog, p1, "add");
            admin.manageProducts(catalog, p2, "add");
            admin.manageProducts(catalog, p3, "add");

            List<Inventory> inventories = new List<Inventory>
            {
                new Inventory(1, 1, 50, 15),
                new Inventory(2, 2, 14, 15),
                new Inventory(3, 3, 25, 10)
            };

            staff.monitorInventory(inventories, catalog);

            Order order1 = new Order(1001);
            order1.AddDetail(new OrderDetail(1, 1001, p1, 2, 10.0)); // Grande (+10)
            order1.AddDetail(new OrderDetail(2, 1001, p2, 1, 20.0)); // More Pearls (+20)
            staff.processOrder(order1, inventories);

            Report salesReport = new Report(501, "Midterm C# Sales & Inventory Report");
            admin.generateReport(salesReport, inventories, new List<Order> { order1 });

            staff.logout();
            admin.logout();
        }
    }
}
