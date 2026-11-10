(function () {
    const countElement = document.getElementById("online-count");

    if (!countElement || typeof signalR === "undefined") {
        return;
    }

    async function fetchPresenceInfo() {
        const response = await fetch("/Presence/Token", { credentials: "same-origin" });

        if (!response.ok) {
            throw new Error("Could not get presence info.");
        }

        return response.json();
    }

    async function start() {
        const info = await fetchPresenceInfo();

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(info.hubUrl, {
                accessTokenFactory: async () => (await fetchPresenceInfo()).token,
                withCredentials: false
            })
            .withAutomaticReconnect()
            .build();

        connection.on("OnlineCountChanged", function (count) {
            countElement.textContent = count;
        });

        await connection.start();
    }

    start().catch(function (error) {
        console.error("Presence connection failed:", error);
    });
})();