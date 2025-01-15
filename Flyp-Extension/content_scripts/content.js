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

    return [Array.from(elements).map((element) => element.innerHTML), hasNextPage];
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
    return document.getElementById("listingUrl").value;
}

async function disableBeforeUnload() {
    window.history.go(-1);
}

/* ******************************************
 *
 * End of injected functions.
 * ------------------------------------------
 * Start of extension functions.
 *
 * *******************************************/

document.addEventListener("DOMContentLoaded", async function () {
    let link = document.getElementById("btn");
    let html_body = "";
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

                            for (const element of elements) {
                                let ebay_id = await getEbayURL(element);
                                html_body += ` next ${element}|${ebay_id}`;
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

        send(html_body);
    });
});

async function getEbayURL(data) {
    let status_span = document.createElement("span");
    status_span.id = "status_span";

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
                    let ebayId;
                    chrome.tabs.update(tabs[0].id, {  }, async () => {
                        // Disable `beforeunload` handlers before navigation
                        await chrome.scripting.executeScript({
                            target: { tabId: tabs[0].id },
                            function: disableBeforeUnload,
                        });
                        chrome.tabs.update(tabs[0].id, { url: toolsUrl }, async () => {
                            await new Promise((resolve) => setTimeout(resolve, 4000)); // Wait for navigation to complete
                            ebayId = await new Promise((resolve, reject) => {
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

                                            if (results && results[0] && results[0].result) {
                                                resolve(results[0].result);
                                            } else {
                                                resolve("N/A");
                                            }
                                        }
                                    );
                                });
                            });
                            status_span.textContent = `Processed Ebay ID: ${ebayId}`;
                            resolve(ebayId);
                        });
                    })
                }
            );
        });
    });
}

function send(data) {
    fetch("http://localhost:60024/", {
        method: "POST",
        body: data
    });
}