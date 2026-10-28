/**
 * PulsePoint HMS - Dashboard Client Script
 * Area: Dashboard
 * Path: Areas/Dashboard/Js/dashboard.js
 */

(function () {
    'use strict';

    // Global PulsePoint Client Helper namespace
    window.PulsePoint = window.PulsePoint || {
        /**
         * Reads stored JWT token from cookies
         */
        getJwtToken: function () {
            const name = 'hms_token=';
            const decodedCookie = decodeURIComponent(document.cookie);
            const ca = decodedCookie.split(';');
            for (let i = 0; i < ca.length; i++) {
                let c = ca[i].trim();
                if (c.indexOf(name) === 0) {
                    return c.substring(name.length, c.length);
                }
            }
            return '';
        },

        /**
         * Makes authenticated API request using JWT Bearer Authorization header
         */
        apiCall: async function (url, options = {}) {
            const token = this.getJwtToken();
            const headers = {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                ...(options.headers || {})
            };

            if (token) {
                headers['Authorization'] = `Bearer ${token}`;
            }

            try {
                const response = await fetch(url, { ...options, headers });
                if (response.status === 401) {
                    console.warn('[PulsePoint] Session expired or unauthorized. Redirecting to login...');
                    window.location.href = '/Security/Auth/Login';
                    return null;
                }
                return await response.json();
            } catch (err) {
                console.error('[PulsePoint] API call failed:', err);
                throw err;
            }
        },

        /**
         * Displays sleek enterprise toast notification
         */
        notify: function (message, type = 'info') {
            const toast = document.createElement('div');
            toast.className = `pp-toast pp-toast-${type}`;
            toast.innerHTML = `<span>${message}</span>`;
            document.body.appendChild(toast);

            setTimeout(() => {
                toast.classList.add('show');
            }, 50);

            setTimeout(() => {
                toast.classList.remove('show');
                setTimeout(() => toast.remove(), 300);
            }, 3500);
        }
    };

    // Initialize Dashboard UI features upon DOM ready
    document.addEventListener('DOMContentLoaded', () => {
        // 1. Live Time Badge Updater
        const timeBadge = document.getElementById('liveClockBadge');
        if (timeBadge) {
            const updateClock = () => {
                const now = new Date();
                timeBadge.textContent = '🕒 ' + now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
            };
            updateClock();
            setInterval(updateClock, 1000);
        }

        // 2. Wire Quick Clinical Action buttons
        document.querySelectorAll('.action-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const actionName = btn.querySelector('strong')?.textContent || 'Action';
                PulsePoint.notify(`Opened clinical workflow: ${actionName}`, 'info');
            });
        });

        // 3. Highlight active menu item based on hash or URL
        const navLinks = document.querySelectorAll('.sidebar-menu .menu-link');
        navLinks.forEach(link => {
            link.addEventListener('click', function () {
                navLinks.forEach(l => l.classList.remove('active'));
                this.classList.add('active');
            });
        });

        console.log('[PulsePoint] Dashboard Area JS initialized. JWT Token available:', Boolean(PulsePoint.getJwtToken()));
    });
})();
