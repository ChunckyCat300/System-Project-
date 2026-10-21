// --- 1. EXISTING PASSWORD TOGGLE LOGIC ---
const toggleBtn = document.getElementById('togglePasswordBtn');
const passwordInput = document.getElementById('password');
const eyeOpen = document.getElementById('eye-open');
const eyeClosed = document.getElementById('eye-closed');

toggleBtn.addEventListener('click', function () {
    const isPassword = passwordInput.getAttribute('type') === 'password';
    passwordInput.setAttribute('type', isPassword ? 'text' : 'password');
    
    if (isPassword) {
        eyeOpen.style.display = 'none';
        eyeClosed.style.display = 'block';
    } else {
        eyeOpen.style.display = 'block';
        eyeClosed.style.display = 'none';
    }
});

// --- 2. NEW LOGIN VERIFICATION LOGIC ---
const loginForm = document.getElementById('loginForm');
const usernameInput = document.getElementById('email'); 
const errorMsg = document.getElementById('error-msg');

loginForm.addEventListener('submit', function (event) {
    // Prevent the form from refreshing the page immediately
    event.preventDefault(); 
    
    // Get the text the user typed
    const enteredUsername = usernameInput.value;
    const enteredPassword = passwordInput.value;

   // --- ADMIN LOGIC ---
    if (enteredUsername === 'admin' && enteredPassword === '1234') {
        localStorage.setItem('activeUser', 'Admin');
        // UPDATE THIS LINE: Route Admin to the POS first
        window.location.href = '../Home Page/index.html'; 
    }
    // --- STAFF LOGIC ---
    else if (enteredUsername === 'staff' && enteredPassword === '1234') {
        localStorage.setItem('activeUser', 'Staff');
        window.location.href = '../Home Page/index.html';
    } 
    // --- ERROR LOGIC ---
    else {
        errorMsg.textContent = 'Invalid username or password.';
        errorMsg.style.display = 'block'; 
        usernameInput.style.borderColor = '#ef4444';
        passwordInput.style.borderColor = '#ef4444';
    }
});

// Hide the error message again as soon as they start typing
usernameInput.addEventListener('input', hideError);
passwordInput.addEventListener('input', hideError);

function hideError() {
    errorMsg.style.display = 'none';
    usernameInput.style.borderColor = 'var(--border-color)';
    passwordInput.style.borderColor = 'var(--border-color)';
}