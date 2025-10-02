document.addEventListener('DOMContentLoaded', function () {
    const form = document.querySelector('form');
    const usernameInput = document.getElementById('Username');
    const usernameError = document.querySelector('span[data-valmsg-for="Username"]');

    if (usernameInput && usernameError) {
        usernameInput.addEventListener('input', function () {
            if (usernameError.textContent !== '') usernameError.textContent = '';
            if (usernameInput.classList.contains('input-validation-error')) {
                usernameInput.classList.remove('input-validation-error', 'is-invalid');
            }
        });
    }

    if (form && usernameInput) {
        form.addEventListener('submit', function (e) {
            const raw = usernameInput.value || '';
            const usernameInputSanitized = (typeof DOMPurify !== 'undefined')
                ? DOMPurify.sanitize(raw, { ALLOWED_TAGS: [], ALLOWED_ATTR: [] }).trim()
                : raw.replace(/<[^>]*>/g, '').trim();
            usernameInput.value = usernameInputSanitized;
            if (!usernameInputSanitized) {
                e.preventDefault();
                if (usernameError) usernameError.textContent = 'Username cannot be empty.';
                usernameInput.classList.add('input-validation-error', 'is-invalid');
            }
        });
    }
});
