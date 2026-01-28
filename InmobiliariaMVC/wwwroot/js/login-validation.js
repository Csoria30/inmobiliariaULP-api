// Validación de cliente para el formulario de login (vanilla JS)
(function () {
    'use strict';

    const form = document.getElementById('loginForm');
    if (!form) return;

    const email = document.getElementById('email');
    const password = document.getElementById('password');

    function validateEmail(value) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
    }

    function clearInvalid(el) {
        el.classList.remove('is-invalid');
    }

    if (email) email.addEventListener('input', () => clearInvalid(email));
    if (password) password.addEventListener('input', () => clearInvalid(password));

    form.addEventListener('submit', function (e) {
        let valid = true;

        if (!email || !email.value || !validateEmail(email.value)) {
            email?.classList.add('is-invalid');
            valid = false;
        }

        if (!password || !password.value) {
            password?.classList.add('is-invalid');
            valid = false;
        }

        if (!valid) {
            e.preventDefault();
            e.stopPropagation();
        }
    });
})();