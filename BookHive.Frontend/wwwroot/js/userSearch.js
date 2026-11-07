(() => {
    const searchContainer = document.querySelector(".user-search-nav");
    const input = document.querySelector("#user-search-input");
    const results = document.querySelector("#user-search-results");
    const status = document.querySelector("#user-search-status");

    if (!(searchContainer instanceof HTMLElement) ||
        !(input instanceof HTMLInputElement) ||
        !(results instanceof HTMLElement) ||
        !(status instanceof HTMLElement)) {
        return;
    }

    const searchUrl = searchContainer.dataset.searchUrl;
    if (!searchUrl) {
        return;
    }

    let debounceTimer;
    let activeRequest;

    const closeResults = () => {
        results.hidden = true;
        input.setAttribute("aria-expanded", "false");
        input.removeAttribute("aria-activedescendant");
    };

    const showMessage = (message) => {
        const paragraph = document.createElement("p");
        paragraph.className = "user-search-message";
        paragraph.textContent = message;
        results.replaceChildren(paragraph);
        results.hidden = false;
        input.setAttribute("aria-expanded", "true");
    };

    const showUsers = (users) => {
        if (users.length === 0) {
            showMessage("No users found.");
            return;
        }

        const list = document.createDocumentFragment();
        users.forEach((user) => {
            const link = document.createElement("a");
            link.className = "user-search-result";
            link.setAttribute("role", "option");
            link.href = user.profileUrl;

            if (user.avatarUrl) {
                const avatar = document.createElement("img");
                avatar.src = user.avatarUrl;
                avatar.alt = "";
                link.append(avatar);
            } else {
                const avatar = document.createElement("span");
                avatar.className = "user-search-result-avatar";
                avatar.setAttribute("aria-hidden", "true");
                avatar.textContent = user.username?.slice(0, 1).toUpperCase() ?? "?";
                link.append(avatar);
            }

            const info = document.createElement("span");
            info.className = "user-search-result-info";
            const username = document.createElement("strong");
            username.textContent = user.username;
            info.append(username);

            if (user.displayName) {
                const displayName = document.createElement("small");
                displayName.textContent = user.displayName;
                info.append(displayName);
            }

            link.append(info);
            list.append(link);
        });

        results.replaceChildren(list);
        results.hidden = false;
        input.setAttribute("aria-expanded", "true");
    };

    input.addEventListener("input", () => {
        window.clearTimeout(debounceTimer);
        activeRequest?.abort();
        results.replaceChildren();

        const query = input.value.trim();
        status.textContent = "";
        if (query.length < 2) {
            closeResults();
            return;
        }

        showMessage("Searching...");
        debounceTimer = window.setTimeout(async () => {
            activeRequest = new AbortController();
            const url = new URL(searchUrl, window.location.origin);
            url.searchParams.set("query", query);

            try {
                const response = await fetch(url, {
                    signal: activeRequest.signal,
                    headers: { Accept: "application/json" }
                });
                if (!response.ok) {
                    throw new Error(`User search failed with status ${response.status}.`);
                }

                const users = await response.json();
                showUsers(users);
                status.textContent = users.length === 0
                    ? "No users found."
                    : `${users.length} user search results available.`;
            } catch (error) {
                if (error.name === "AbortError") {
                    return;
                }

                console.error(error);
                showMessage("Search failed. Please try again.");
            }
        }, 250);
    });

    input.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            closeResults();
        }
    });

    document.addEventListener("click", (event) => {
        if (!searchContainer.contains(event.target)) {
            closeResults();
        }
    });
})();
