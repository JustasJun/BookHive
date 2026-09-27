document.addEventListener('DOMContentLoaded', () => {
    const API_BASE_URL = 'http://localhost:5254';

    const form = document.getElementById('searchForm');
    const resultsDiv = document.getElementById('results');

    const genreToggle = document.getElementById('genreDropdownToggle');
    const genreMenu = document.getElementById('genreDropdownMenu');
    const genreCount = document.getElementById('genreCount');

    genreToggle.addEventListener('click', () => {
        genreMenu.classList.toggle('open');
    });

    document.addEventListener('click', (e) => {
        if (!document.getElementById('genreDropdown').contains(e.target)) {
            genreMenu.classList.remove('open');
        }
    });

    document.querySelectorAll('.genre-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', updateGenreCount);
    });

    function updateGenreCount() {
        const checked = document.querySelectorAll('.genre-checkbox:checked').length;
        genreCount.textContent = checked > 0 ? `(${checked})` : '';
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        await searchBooks();
    });

    async function searchBooks() {
        const query = document.getElementById('queryInput').value.trim();
        const checkedGenres = Array.from(
            document.querySelectorAll('.genre-checkbox:checked')
        ).map(checkbox => checkbox.value);
        const sortBy = document.getElementById('sortBySelect').value;

        if (!query) {
            resultsDiv.innerHTML = '<p>Įveskite paieškos žodį.</p>';
            return;
        }

        resultsDiv.innerHTML = '<p>Ieškoma...</p>';

        const params = new URLSearchParams({ query });
        checkedGenres.forEach(genre => params.append('genres', genre));
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