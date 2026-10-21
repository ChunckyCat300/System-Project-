document.addEventListener('DOMContentLoaded', () => {
    // ==========================================
    // 1. SETUP PROFILE & LOGOUT
    // ==========================================
    const activeUser = localStorage.getItem('activeUser') || 'Admin';
    document.getElementById('user-profile-name').textContent = activeUser;
    
    document.getElementById('logout-btn').addEventListener('click', () => {
        localStorage.removeItem('activeUser');
    });

    // ==========================================
    // 2. LOAD DATA & DOM ELEMENTS
    // ==========================================
    const allSales = JSON.parse(localStorage.getItem('posSales')) || [];
    
    const revenueDisplay = document.getElementById('total-revenue');
    const ordersDisplay = document.getElementById('total-orders');
    const tableBody = document.getElementById('sales-table-body');
    const filterLabel = document.getElementById('current-filter-label');
    const filterBtns = document.querySelectorAll('.filter-btn');

    // ==========================================
    // 3. DATE HELPER FUNCTIONS
    // ==========================================
    const now = new Date();
    
    function isSameDay(d1, d2) {
        return d1.getFullYear() === d2.getFullYear() &&
               d1.getMonth() === d2.getMonth() &&
               d1.getDate() === d2.getDate();
    }

    // Calculates the ISO week number for accurate weekly filtering
    function getWeekNumber(d) {
        d = new Date(Date.UTC(d.getFullYear(), d.getMonth(), d.getDate()));
        d.setUTCDate(d.getUTCDate() + 4 - (d.getUTCDay()||7));
        var yearStart = new Date(Date.UTC(d.getUTCFullYear(),0,1));
        return Math.ceil(( ( (d - yearStart) / 86400000) + 1)/7);
    }

    function isSameWeek(d1, d2) {
        return d1.getFullYear() === d2.getFullYear() &&
               getWeekNumber(d1) === getWeekNumber(d2);
    }

    function isSameMonth(d1, d2) {
        return d1.getFullYear() === d2.getFullYear() &&
               d1.getMonth() === d2.getMonth();
    }

    function isSameYear(d1, d2) {
        return d1.getFullYear() === d2.getFullYear();
    }

    // ==========================================
    // 4. RENDER DASHBOARD UI
    // ==========================================
    function renderDashboard(filterType) {
        let filteredSales = [];
        
        // Filter the sales array based on the requested date range
        allSales.forEach(sale => {
            const saleDate = new Date(sale.date);
            let include = false;
            
            if (filterType === 'daily') include = isSameDay(saleDate, now);
            else if (filterType === 'weekly') include = isSameWeek(saleDate, now);
            else if (filterType === 'monthly') include = isSameMonth(saleDate, now);
            else if (filterType === 'annually') include = isSameYear(saleDate, now);
            else if (filterType === 'all') include = true;

            if (include) filteredSales.push(sale);
        });

        // Calculate total revenue for this period
        let totalRev = 0;
        filteredSales.forEach(s => totalRev += s.total);
        
        // Update summary cards
        revenueDisplay.textContent = `₱${totalRev.toFixed(2)}`;
        ordersDisplay.textContent = filteredSales.length;

        // Render the table
        tableBody.innerHTML = '';
        if (filteredSales.length === 0) {
            tableBody.innerHTML = `<tr><td colspan="4" style="text-align: center; padding: 30px; color: #64748b;">No transactions found for this period.</td></tr>`;
        } else {
            // Reverse the array so the newest sales appear at the top
            filteredSales.reverse().forEach(sale => {
                const d = new Date(sale.date);
                const dateStr = d.toLocaleDateString() + ' ' + d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
                
                // Calculate total items inside this specific order
                const itemCount = sale.items ? sale.items.reduce((sum, item) => sum + item.quantity, 0) : 0;

                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>${dateStr}</td>
                    <td>${sale.cashier}</td>
                    <td>${itemCount} items</td>
                    <td class="amount-cell">₱${sale.total.toFixed(2)}</td>
                `;
                tableBody.appendChild(tr);
            });
        }
    }

    // ==========================================
    // 5. EVENT LISTENERS
    // ==========================================
    filterBtns.forEach(btn => {
        btn.addEventListener('click', (e) => {
            // Remove active style from all buttons
            filterBtns.forEach(b => b.classList.remove('active'));
            // Add active style to clicked button
            e.target.classList.add('active');
            
            // Get filter string (daily, weekly, etc.)
            const filter = e.target.getAttribute('data-filter');
            filterLabel.textContent = `(${e.target.textContent})`;
            
            // Re-render UI
            renderDashboard(filter);
        });
    });

    // Initialize UI on page load (Default to Daily)
    renderDashboard('daily');
});

// ==========================================
    // 6. SIDEBAR NAVIGATION TOGGLE
    // ==========================================
    const sidebarLinks = document.querySelectorAll('.sidebar-link');
    const dashboardSections = document.querySelectorAll('.dashboard-section');

    sidebarLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault(); // Prevents the page from jumping to the top

            // Remove active styling from all links
            sidebarLinks.forEach(l => l.classList.remove('active'));
            // Add active styling to the clicked link
            e.target.classList.add('active');

            // Hide all content sections
            dashboardSections.forEach(section => {
                section.style.display = 'none';
            });

            // Show only the section that matches the clicked link's data-target
            const targetId = e.target.getAttribute('data-target');
            document.getElementById(targetId).style.display = 'block';
        });
    });

    // ==========================================
    // 7. INVENTORY, STAFF & SETTINGS LOGIC
    // ==========================================
    
    // Restock Buttons
    const restockBtns = document.querySelectorAll('.restock-btn');
    restockBtns.forEach(btn => {
        btn.addEventListener('click', (e) => {
            const itemName = e.target.closest('tr').querySelector('td').textContent;
            alert(`Purchase order generated to restock: ${itemName}`);
        });
    });

    // Reset Password Buttons
    const resetPassBtns = document.querySelectorAll('.reset-pass-btn');
    resetPassBtns.forEach(btn => {
        btn.addEventListener('click', (e) => {
            const username = e.target.closest('tr').querySelector('td').textContent;
            const newPass = prompt(`Enter new password for user '${username}':`);
            if (newPass) {
                alert(`Password for '${username}' has been successfully updated.`);
            }
        });
    });

    // Save Settings
    const saveSettingsBtn = document.getElementById('save-settings-btn');
    if (saveSettingsBtn) {
        saveSettingsBtn.addEventListener('click', () => {
            const msg = document.getElementById('settings-msg');
            msg.style.display = 'block';
            setTimeout(() => {
                msg.style.display = 'none';
            }, 3000);
        });
    }