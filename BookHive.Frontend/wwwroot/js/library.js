document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.remove-from-library-btn').forEach(button => {
        button.addEventListener('click', async () => {
            const bookId = button.dataset.bookId;

            try {
                const token = document.querySelector(
                    'input[name="__RequestVerificationToken"]'
                )?.value;

                if (!token) {
                    throw new Error('Antiforgery token is missing.');
                }

                const response = await fetch(
                    `/Library/Remove?bookId=${encodeURIComponent(bookId)}`,
                    {
                        method: 'POST',
                        headers: {
                            'RequestVerificationToken': token
                        }
                    }
                );

                if (!response.ok) {
                    throw new Error(`Server error: ${response.status}`);
                }

                button.closest('.book-card').remove();
            } catch (err) {
                console.error(err);
                alert('Failed to remove book.');
            }
        });
    });
});