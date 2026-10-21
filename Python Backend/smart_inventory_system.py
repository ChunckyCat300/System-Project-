"""
Smart Inventory Monitoring and Order Management System (Moon & Mug)
PRELIM PERIOD - Python Object-Oriented Programming (OOP) Implementation
Matches the Use Class Diagram & Functional Analysis in the Project Documentation.
"""

from datetime import datetime
from typing import List, Optional


# ==========================================
# 1. USER, ADMINISTRATOR & STAFF (INHERITANCE & ENCAPSULATION)
# ==========================================
class User:
    def __init__(self, user_id: int, username: str, password: str, full_name: str, role: str):
        self._userID = user_id
        self._username = username
        self._password = password
        self._fullName = full_name
        self._role = role
        self._is_logged_in = False

    @property
    def userID(self) -> int:
        return self._userID

    @property
    def username(self) -> str:
        return self._username

    @property
    def fullName(self) -> str:
        return self._fullName

    @property
    def role(self) -> str:
        return self._role

    def login(self, entered_username: str, entered_password: str) -> bool:
        if self._username == entered_username and self._password == entered_password:
            self._is_logged_in = True
            print(f"[LOGIN SUCCESS] Welcome, {self._fullName} ({self._role})!")
            return True
        print("[LOGIN ERROR] Invalid username or password.")
        return False

    def logout(self) -> None:
        self._is_logged_in = False
        print(f"[LOGOUT] User '{self._username}' has logged out.")


class Administrator(User):
    def __init__(self, user_id: int, username: str, password: str, full_name: str, admin_id: int, position: str = "Store Administrator"):
        super().__init__(user_id, username, password, full_name, role="Admin")
        self.adminID = admin_id
        self.position = position

    def manageUsers(self, staff_list: List['Staff'], action: str, staff_obj: 'Staff') -> None:
        if action == "add":
            staff_list.append(staff_obj)
            print(f"[ADMIN - STAFF MGMT] Added staff account: {staff_obj.username} ({staff_obj.position})")
        elif action == "deactivate":
            print(f"[ADMIN - STAFF MGMT] Deactivated staff account: {staff_obj.username}")

    def manageProducts(self, catalog: List['Product'], action: str, product: 'Product') -> None:
        if action == "add":
            product.addProduct()
            catalog.append(product)
        elif action == "update":
            product.updateProduct(product.productName, product.category, product.price)
        elif action == "delete":
            product.deleteProduct()
            if product in catalog:
                catalog.remove(product)

    def generateReport(self, report: 'Report', inventories: List['Inventory'], orders: List['Order']) -> None:
        print(f"\n[ADMIN - REPORT] Generating '{report.reportType}' Report by {self.fullName}...")
        report.generateReport(inventories, orders)
        report.viewReport()


class Staff(User):
    def __init__(self, user_id: int, username: str, password: str, full_name: str, staff_id: int, position: str = "Cashier / Barista"):
        super().__init__(user_id, username, password, full_name, role="Staff")
        self.staffID = staff_id
        self.position = position

    def processOrder(self, order: 'Order', inventories: List['Inventory']) -> None:
        print(f"\n[STAFF - POS] {self.fullName} is processing Order #{order.orderID}...")
        order.createOrder()
        for detail in order.orderDetails:
            for inv in inventories:
                if inv.productID == detail.productID:
                    inv.updateStock(-detail.quantity)
        order.updateOrder("Completed")

    def updateStock(self, inventory: 'Inventory', quantity_change: int) -> None:
        print(f"[STAFF - STOCK UPDATE] {self.fullName} updating stock for Product ID #{inventory.productID}...")
        inventory.updateStock(quantity_change)

    def monitorInventory(self, inventories: List['Inventory'], products: List['Product']) -> None:
        print(f"\n--- INVENTORY MONITOR (Checked by {self.fullName}) ---")
        for inv in inventories:
            prod_name = next((p.productName for p in products if p.productID == inv.productID), f"Product #{inv.productID}")
            status = "LOW STOCK ALERT!" if inv.checkLowStock() else "OK"
            print(f"ID: {inv.inventoryID} | Item: {prod_name:<25} | Stock: {inv.checkStock():<4} | Reorder Level: {inv.reorderLevel:<3} | Status: {status}")


# ==========================================
# 2. PRODUCT & INVENTORY CLASSES
# ==========================================
class Product:
    def __init__(self, product_id: int, product_name: str, category: str, price: float):
        self.productID = product_id
        self.productName = product_name
        self.category = category
        self.price = price

    def addProduct(self) -> None:
        print(f"[PRODUCT ADDED] #{self.productID}: {self.productName} ({self.category}) - PHP {self.price:.2f}")

    def updateProduct(self, new_name: str, new_category: str, new_price: float) -> None:
        self.productName = new_name
        self.category = new_category
        self.price = new_price
        print(f"[PRODUCT UPDATED] #{self.productID}: {self.productName} - PHP {self.price:.2f}")

    def deleteProduct(self) -> None:
        print(f"[PRODUCT DELETED] #{self.productID}: {self.productName} removed from catalog.")


class Inventory:
    def __init__(self, inventory_id: int, product_id: int, quantity: int, reorder_level: int):
        self.inventoryID = inventory_id
        self.productID = product_id
        self.quantity = quantity
        self.reorderLevel = reorder_level

    def updateStock(self, amount: int) -> None:
        self.quantity += amount
        if self.quantity < 0:
            self.quantity = 0
        print(f"[INVENTORY #{self.inventoryID}] Product #{self.productID} stock updated to {self.quantity} units.")
        if self.checkLowStock():
            print(f"  -> [WARNING] Product #{self.productID} is at or below reorder level ({self.reorderLevel})!")

    def checkStock(self) -> int:
        return self.quantity

    def checkLowStock(self) -> bool:
        return self.quantity <= self.reorderLevel


# ==========================================
# 3. ORDER, ORDER DETAIL & REPORT CLASSES
# ==========================================
class OrderDetail:
    def __init__(self, order_detail_id: int, order_id: int, product: Product, quantity: int, size: str = "Tall", topping: str = "Pearls"):
        self.orderDetailID = order_detail_id
        self.orderID = order_id
        self.productID = product.productID
        self.productName = product.productName
        self.unitPrice = product.price
        self.quantity = quantity
        self.size = size
        self.topping = topping
        self.subtotal = self.calculateTotal()

    def calculateTotal(self) -> float:
        addon = 0.0
        if self.size == "Grande":
            addon += 10.0
        elif self.size == "Venti":
            addon += 20.0
        if self.topping == "More Pearls":
            addon += 20.0
        self.subtotal = (self.unitPrice + addon) * self.quantity
        return self.subtotal


class Order:
    def __init__(self, order_id: int, order_date: Optional[str] = None):
        self.orderID = order_id
        self.orderDate = order_date or datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        self.totalAmount = 0.0
        self.status = "Pending"
        self.orderDetails: List[OrderDetail] = []

    def addOrderDetail(self, detail: OrderDetail) -> None:
        self.orderDetails.append(detail)
        self.totalAmount = sum(d.calculateTotal() for d in self.orderDetails)

    def createOrder(self) -> None:
        self.totalAmount = sum(d.calculateTotal() for d in self.orderDetails)
        self.status = "Processing"
        print(f"[ORDER CREATED] Order #{self.orderID} on {self.orderDate} | Total: PHP {self.totalAmount:.2f}")

    def updateOrder(self, new_status: str) -> None:
        self.status = new_status
        print(f"[ORDER STATUS] Order #{self.orderID} status updated to: {self.status}")

    def cancelOrder(self) -> None:
        self.status = "Cancelled"
        print(f"[ORDER CANCELLED] Order #{self.orderID} has been cancelled.")


class Report:
    def __init__(self, report_id: int, report_type: str):
        self.reportID = report_id
        self.reportType = report_type
        self.dateGenerated = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        self._content = ""

    def generateReport(self, inventories: List[Inventory], orders: List[Order]) -> None:
        completed_orders = [o for o in orders if o.status == "Completed"]
        total_revenue = sum(o.totalAmount for o in completed_orders)
        low_stock_items = [inv for inv in inventories if inv.checkLowStock()]

        lines = [
            "==========================================================",
            f" MOON & MUG - {self.reportType.upper()}",
            f" Report ID: #{self.reportID} | Generated: {self.dateGenerated}",
            "==========================================================",
            f" Total Completed Orders : {len(completed_orders)}",
            f" Total Sales Revenue    : PHP {total_revenue:.2f}",
            f" Low-Stock Alerts Count : {len(low_stock_items)}",
            "----------------------------------------------------------",
            "Order History:"
        ]
        for o in completed_orders:
            lines.append(f"  - Order #{o.orderID} ({o.orderDate}) | Status: {o.status} | PHP {o.totalAmount:.2f}")
        lines.append("==========================================================")
        self._content = "\n".join(lines)

    def viewReport(self) -> None:
        print(self._content)


# ==========================================
# MAIN EXECUTION (SIMULATION OF MOON & MUG POS)
# ==========================================
if __name__ == "__main__":
    print("=== SMART INVENTORY MONITORING AND ORDER MANAGEMENT SYSTEM (PYTHON) ===\n")

    # 1. Initialize Users
    admin = Administrator(1, "admin", "1234", "Brics S. Coyoca (Admin)", admin_id=101)
    cashier = Staff(2, "staff", "1234", "John Carlo Barbero (Staff)", staff_id=201)
    staff_accounts = []

    # 2. Authenticate Users
    admin.login("admin", "1234")
    cashier.login("staff", "1234")

    # 3. Admin Manages Staff & Product Catalog
    admin.manageUsers(staff_accounts, "add", cashier)

    catalog: List[Product] = []
    p1 = Product(1, "Classic Milk Tea", "Milk Tea", 90.00)
    p2 = Product(2, "Okinawa Milk Tea", "Milk Tea", 110.00)
    p3 = Product(3, "Coffee Frappe", "Frappe", 140.00)
    p4 = Product(4, "Americano", "Hot Coffee", 85.00)

    for prod in [p1, p2, p3, p4]:
        admin.manageProducts(catalog, "add", prod)

    # 4. Initialize Inventory
    inventories = [
        Inventory(1, p1.productID, quantity=50, reorder_level=15),
        Inventory(2, p2.productID, quantity=12, reorder_level=15),  # Already low stock
        Inventory(3, p3.productID, quantity=30, reorder_level=10),
        Inventory(4, p4.productID, quantity=40, reorder_level=10),
    ]

    # 5. Staff Monitors Inventory
    cashier.monitorInventory(inventories, catalog)

    # 6. Staff Processes a Customer Order
    order1 = Order(order_id=1001)
    order1.addOrderDetail(OrderDetail(1, order1.orderID, p1, quantity=2, size="Grande", topping="Pearls"))
    order1.addOrderDetail(OrderDetail(2, order1.orderID, p2, quantity=1, size="Tall", topping="More Pearls"))
    cashier.processOrder(order1, inventories)

    # 7. Admin Generates System Report
    sales_report = Report(report_id=501, report_type="Daily Sales & Inventory Summary")
    admin.generateReport(sales_report, inventories, [order1])

    # 8. Logout
    cashier.logout()
    admin.logout()
