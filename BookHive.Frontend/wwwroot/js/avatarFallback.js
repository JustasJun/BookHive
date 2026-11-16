document.addEventListener("error", (event) => {
    const image = event.target;
    if (!(image instanceof HTMLImageElement) || !image.dataset.avatarFallback) {
        return;
    }

    const fallback = document.createElement("span");
    fallback.className = image.dataset.avatarFallbackClass
        ? `${image.className} ${image.dataset.avatarFallbackClass}`
        : image.className;
    fallback.setAttribute("aria-hidden", "true");
    fallback.textContent = image.dataset.avatarFallback;
    image.replaceWith(fallback);
}, true);
