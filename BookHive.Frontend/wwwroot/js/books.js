document.addEventListener('DOMContentLoaded', () => {
    const API_BASE_URL = 'http://localhost:5254';

    const form = document.getElementById('searchForm');
    const resultsDiv = document.getElementById('results');

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        await searchBooks();
    });

    async function searchBooks() {
        const query = document.getElementById('queryInput').value.trim();
        const genre = document.getElementById('genreSelect').value;
        const sortBy = document.getElementById('sortBySelect').value;

        if (!query) {
            resultsDiv.innerHTML = '<p>Įveskite paieškos žodį.</p>';
            return;
        }

        resultsDiv.innerHTML = '<p>Ieškoma...</p>';

        const params = new URLSearchParams({ query });
        if (genre) params.append('genre', genre);
        if (sortBy) params.append('sortBy', sortBy);

        try {
            const response = await fetch(`${API_BASE_URL}/api/books?${params.toString()}`);

            if (!response.ok) {
                throw new Error(`Serverio klaida: ${response.status}`);
            }

            const books = await response.json();
            renderBooks(books);
        } catch (err) {
            resultsDiv.innerHTML = '<p>Įvyko klaida ieškant knygų. Bandykite dar kartą.</p>';
            console.error(err);
        }
    }

    function renderBooks(books) {
        if (books.length === 0) {
            resultsDiv.innerHTML = '<p>Nieko nerasta. Pabandyk paiešką.</p>';
            return;
        }

        resultsDiv.innerHTML = books.map(book => `
            <div class="book-card">
                <div class="book-info">
                    <h3>${escapeHtml(book.title)}</h3>
                    <p>Autorius: ${escapeHtml(book.author)}</p>
                    <p>Metai: ${book.firstPublishYear ?? '—'}</p>
                    <p>Žanras: ${book.genre}</p>
                </div>
                <img src="${book.cover.url}" alt="${escapeHtml(book.title)} viršelis" class="book-cover" />
            </div>
        `).join('');
    }

    function escapeHtml(str) {
        const div = document.createElement('div');
        div.textContent = str ?? '';
        return div.innerHTML;
    }
});