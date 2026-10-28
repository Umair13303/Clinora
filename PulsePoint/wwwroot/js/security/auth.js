// PulsePoint HMS Client Script - Security Area
document.addEventListener('DOMContentLoaded', () => {
    // 1. Password Visibility Toggle
    const toggleBtn = document.getElementById('togglePasswordBtn');
    const pwdInput = document.getElementById('passwordInput');
    if (toggleBtn && pwdInput) {
        toggleBtn.addEventListener('click', () => {
            const isPassword = pwdInput.type === 'password';
            pwdInput.type = isPassword ? 'text' : 'password';
            toggleBtn.textContent = isPassword ? '🙈' : '👁️';
        });
    }

    // 2. Remember branch in localStorage & show loading state on submission
    const loginForm = document.getElementById('loginForm');
    const signInBtn = document.getElementById('signInBtn');

    if (loginForm) {
        loginForm.addEventListener('submit', () => {
            const branchSelect = document.getElementById('locationSelect');
            if (branchSelect && branchSelect.selectedIndex >= 0) {
                const branchName = branchSelect.options[branchSelect.selectedIndex].text;
                localStorage.setItem('pulsepoint_branch', branchName);
            }

            if (signInBtn) {
                signInBtn.textContent = 'Authenticating...';
                signInBtn.style.opacity = '0.8';
                signInBtn.style.pointerEvents = 'none';
            }
        });
    }
});
