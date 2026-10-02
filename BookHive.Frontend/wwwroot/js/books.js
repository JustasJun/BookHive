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
            resultsDiv.innerHTML = '<p>Nieko nerasta. Pabandyk kitą paiešką.</p>';
            return;
        }

        resultsDiv.innerHTML = books.map(book => `
            <div class="book-card">
                <div class="book-info">
                    <h3>${escapeHtml(book.title)}</h3>
                    <p>Autorius: ${escapeHtml(book.author)}</p>
                    <p>Metai: ${book.firstPublishYear ?? '—'}</p>
                    <p>Žanras: ${book.genre}</p>
                    <div class="book-actions">
                        <button class="add-to-library-btn" data-book='${JSON.stringify(book)}'>
                            + Į biblioteką
                        </button>
                        <a class="write-review-btn" href="${reviewUrl(book)}">Write a review</a>
                    </div>
                </div>
                <img src="${book.cover.url}" alt="${escapeHtml(book.title)} viršelis" class="book-cover" />
            </div>
        `).join('');

        document.querySelectorAll('.add-to-library-btn').forEach(button => {
            button.addEventListener('click', async () => {
                const book = JSON.parse(button.dataset.book);
                await addToLibrary(book, button);
            });
        });
    }

    async function addToLibrary(book, button) {
        try {
            const response = await fetch(`${API_BASE_URL}/api/library`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    userId: Number(window.currentUserId),
                    book: book,
                    status: 'WantToRead'
                })
            });

            if (!response.ok) {
                throw new Error(`Serverio klaida: ${response.status}`);
            }

            button.textContent = 'Pridėta ✓';
            button.disabled = true;
        } catch (err) {
            console.error(err);
            alert('Nepavyko pridėti knygos.');
        }
    }

    function escapeHtml(str) {
        const div = document.createElement('div');
        div.textContent = str ?? '';
        return div.innerHTML;
    }

    function reviewUrl(book) {
        const params = new URLSearchParams({
            bookId: book.id,
            title: book.title,
            author: book.author
        });
        if (book.cover && book.cover.coverId) params.append('coverId', book.cover.coverId);
            return `/Reviews/Write?${params.toString()}`;
    }
});