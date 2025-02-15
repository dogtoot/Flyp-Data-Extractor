/* ******************************************
 *
 * Start of injected functions.
 *
 * ******************************************/

/**
 * Returns an array of html elements, as well as a boolean indicating the status of the next page.
 * */
async function getHtml() {
    while (document.getElementsByClassName("ant-list ant-list-split").length === 0) {
        await new Promise((resolve) => setTimeout(resolve, 1000));
    }

    let elements = document.getElementsByClassName("single-item-list-container");

    const next_btn = document.querySelector('[title="Next Page"] .ant-pagination-item-link');
    const hasNextPage = next_btn && !next_btn.disabled;

    // Uncomment this if you want to click the next button.
    // if (hasNextPage) {
    //     next_btn.click();
    // }

    return [Array.from(elements).map((element) => element.innerHTML), false];
}

/**
 * Returns a Flyp item url.
 * */
function getFlypItem(element) {
    const start_index = element.indexOf("/item/") + 6; // Adjust index for start of item ID
    const end_index = element.indexOf("\"", start_index);
    const substring = element.substring(start_index, end_index);

    return {
        toolsUrl: `https://tools.joinflyp.com/item/${substring}?marketplace=ebay`
    };
}

/**
 * Returns the ebay listing url.
 * */
function getEbayLink() {
    let brand = "";
    let container_list = document.getElementsByClassName("item-info-input-container")
    for (const container of container_list) {
        try{
            if(container.getElementsByClassName("item-info-label")[0].innerText.includes("Brand")) {
                brand = container.getElementsByClassName("ant-select-selection-item")[0].innerText;
            }
        }
        catch(e) {}
    }

    return {
        url: document.getElementById("listingUrl").value,
        brand: brand
    };
}

/* ******************************************
 *
 * End of injected functions.
 * ------------------------------------------
 * Start of extension functions.
 *
 * *******************************************/

document.addEventListener("DOMContentLoaded", async function () {
    let link = document.getElementById("Flybtn");
    let html_body = "flyp ";
    let next_page = true;

    link.addEventListener("click", async function () {
        while (next_page) {
            await new Promise((resolve, reject) => {
                chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
                    chrome.scripting.executeScript(
                        {
                            target: { tabId: tabs[0].id },
                            function: getHtml,
                        },
                        async (result) => {
                            if (chrome.runtime.lastError) {
                                console.error(chrome.runtime.lastError);
                                return reject(chrome.runtime.lastError);
                            }

                            const [elements, hasNextPage] = result[0].result;
                            next_page = hasNextPage;

                            for (const element of elements) {
                                let [ebay_id, brand] = await getEbayURL(element);
                                html_body = ` flyp ${element}|${ebay_id}|${brand}`;
                                send(html_body);
                                console.log(ebay_id + " | " + brand);
                            }

                            next_page = hasNextPage;
                            resolve();
                        }
                    );
                });
            });

            // Add a delay between pages to avoid overwhelming the site
            await new Promise((resolve) => setTimeout(resolve, 750));
        }
    });
});

async function getEbayURL(data) {
    return new Promise((resolve, reject) => {
        chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
            chrome.scripting.executeScript(
                {
                    args: [data],
                    target: { tabId: tabs[0].id },
                    function: getFlypItem,
                },
                async (results) => {
                    if (chrome.runtime.lastError) {
                        console.error(chrome.runtime.lastError);
                        return reject(chrome.runtime.lastError);
                    }

                    const { toolsUrl } = results[0].result;
                    console.log(`Navigating to: ${toolsUrl}`);
                    chrome.tabs.update(tabs[0].id, { url: toolsUrl }, async () => {
                        await new Promise((resolve) => setTimeout(resolve, 4000)); // Wait for navigation to complete
                        let [ebayId, brand] = await new Promise((resolve, reject) => {
                            chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
                                chrome.scripting.executeScript(
                                    {
                                        target: { tabId: tabs[0].id },
                                        function: getEbayLink
                                    },
                                    (results) => {
                                        if (chrome.runtime.lastError) {
                                            console.error(chrome.runtime.lastError);
                                            return reject(chrome.runtime.lastError);
                                        }

                                        let {url, brand} = results[0].result;

                                        if(brand === "Michael"){
                                            brand = "Michael Kors"
                                        }

                                        if (results && results[0] && results[0].result) {
                                            resolve([url || "N/A", brand || "N/A"]);
                                        } else {
                                            resolve(["N/A", "N/A"]);
                                        }
                                    }
                                );
                            });
                        });
                        // Display a success message to the extension window.
                        document.getElementById("status_span").textContent = `Retrieved Ebay Listing ID: ${ebayId.toString().replace("https://www.ebay.com/itm/", "")}`;
                        resolve([ebayId || "N/A", brand || "N/A"]);
                    });
                }
            );
        });
    });
}

function send(data) {
    fetch("http://localhost:60024/ProcessRequest", { // Replace with server URL.
        method: "POST",
        body: data
    }).then(() => console.log("Successfully sent data."));
}
