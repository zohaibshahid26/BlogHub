const connection = new signalR.HubConnectionBuilder()
    .withUrl("/ChatHub")
    .build();

connection.on("ReceiveMessage", (postTitle, categoryName, imageUrl,postUrl) => {
    Toastify({
        text: `
            <a href="${postUrl}" style="text-decoration: none; color: inherit; display: block; width: 100%; height: 100%; padding: 15px; border-radius: 8px; border: 1px solid #ddd; background-color: #f9f9f9; color: black; font-family: Arial, sans-serif;">
                <div style="display: flex; align-items: center;">
                    <img src="/${imageUrl}" alt="Post Image" style="width: 50px; height: auto; margin-right: 10px; border-radius: 4px;" />
                    <div>
                        <span style="font-size: 14px; color: #555;">
                            <strong>A new post was added in category <span style="color: #007bff;">${categoryName}</span></strong>
                        </span><br>
                        <span style="font-size: 18px; color: #333; font-weight: bold;">
                            <strong>${postTitle}</strong>
                        </span><br>
                    </div>
                </div>
            </a>
        `,
        duration: 15000,
        gravity: "bottom",
        position: "right",
        backgroundColor: "transparent", // Ensure background is transparent
        stopOnFocus: true,
        escapeMarkup: false, // Allow HTML formatting
    }).showToast();
});


connection.start()
    .then(() => console.log("SignalR Connected"))
    .catch(err => console.error("SignalR Connection Error: ", err));
