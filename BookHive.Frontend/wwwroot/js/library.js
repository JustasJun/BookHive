document.addEventListener('DOMContentLoaded', () => {
    const API_BASE_URL = 'http://localhost:5254';

    document.querySelectorAll('.remove-from-library-btn').forEach(button => {
        button.addEventListener('click', async () => {
            const bookId = button.dataset.bookId;
            const userId = window.currentUserId;

            try {
                const response = await fetch(
                    `${API_BASE_URL}/api/library?userId=${userId}&bookId=${encodeURIComponent(bookId)}`,
                    { method: 'DELETE' }
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