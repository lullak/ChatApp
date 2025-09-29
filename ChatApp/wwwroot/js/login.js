document.addEventListener('DOMContentLoaded', function () {
    const usernameInput = document.getElementById('Username');
    const usernameError = document.querySelector('span[data-valmsg-for="Username"]');

    if (usernameInput && usernameError) {

        usernameInput.addEventListener('input', function () {

            if (usernameError.textContent !== '') {
                usernameError.textContent = '';
            }

            if (usernameInput.classList.contains('input-validation-error')) {
                usernameInput.classList.remove('input-validation-error');
                usernameInput.classList.remove('is-invalid');
            }
        });
    }
});
