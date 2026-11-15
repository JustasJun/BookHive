(() => {
    const dialog = document.querySelector("#follow-list-dialog");
    const title = document.querySelector("#follow-list-title");
    const content = dialog?.querySelector(".follow-list-content");
    const closeButton = dialog?.querySelector(".follow-list-close");

    if (!(dialog instanceof HTMLDialogElement) ||
        !(title instanceof HTMLElement) ||
        !(content instanceof HTMLElement) ||
        !(closeButton instanceof HTMLButtonElement)) {
        return;
    }

    let activeRequest;

    const showMessage = (message) => {
        const paragraph = document.createElement("p");
        paragraph.className = "follow-list-message";
        paragraph.textContent = message;
        content.replaceChildren(paragraph);
    };

    const showUsers = (users) => {
        if (users.length === 0) {
            showMessage("No users found.");
            return;
        }

        const list = document.createDocumentFragment();
        users.forEach((user) => {
            const link = document.createElement("a");
            link.className = "follow-list-user";
            link.href = user.profileUrl;

            if (user.avatarUrl) {
                const avatar = document.createElement("img");
                avatar.className = "follow-list-avatar";
                avatar.src = user.avatarUrl;
                avatar.alt = "";
                link.append(avatar);
            } else {
                const avatar = document.createElement("span");
                avatar.className = "follow-list-avatar";
                avatar.setAttribute("aria-hidden", "true");
                avatar.textContent = user.username?.slice(0, 1).toUpperCase() ?? "?";
                link.append(avatar);
            }

            const info = document.createElement("span");
            info.className = "follow-list-user-info";
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

        content.replaceChildren(list);
    };

    document.querySelectorAll(".stat-item--button[data-list-url]").forEach((button) => {
        button.addEventListener("click", async () => {
            activeRequest?.abort();
            activeRequest = new AbortController();
            title.textContent = button.dataset.listTitle ?? "";
            showMessage("Loading...");
            if (!dialog.open) {
                dialog.showModal();
            }

            try {
                const response = await fetch(button.dataset.listUrl, {
                    signal: activeRequest.signal,
                    headers: { Accept: "application/json" }
                });
                if (!response.ok) {
                    throw new Error(`Could not load follow list (${response.status}).`);
                }

                showUsers(await response.json());
            } catch (error) {
                if (error.name === "AbortError") {
                    return;
                }

                console.error(error);
                showMessage("Could not load this list. Please try again.");
            }
        });
    });

    closeButton.addEventListener("click", () => dialog.close());
    dialog.addEventListener("close", () => activeRequest?.abort());
})();
