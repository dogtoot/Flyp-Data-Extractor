/* Start of Injected Functions */

async function getPoshHtml(){
    let cont = true;
    let scroll = 0;
    while(true){
        window.scrollBy(0, 10000);
        if(window.scrollY === scroll){
            break;
        }
        await new Promise((resolve) => setTimeout(resolve, 750));
        scroll = window.scrollY;
    }

    console.log("Bottom");
    let elements = document.getElementsByClassName("card card--small tile");
    return Array.from(elements).map((element) => element.innerHTML);
}

/* End of Injected Functions */

document.addEventListener("DOMContentLoaded", async function () {
    let link = document.getElementById("Poshbtn");
    let html_body = " ";
    link.addEventListener("click", async function () {
        console.log("Haerl!");
        await new Promise((resolve, reject) => {
            chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
                chrome.scripting.executeScript(
                    {
                        target: { tabId: tabs[0].id },
                        function: getPoshHtml,
                    },
                    async (result) => {
                        if (chrome.runtime.lastError) {
                            console.error(chrome.runtime.lastError);
                            return reject(chrome.runtime.lastError);
                        }

                        const [elements] = result[0].result;

                        for (const element of elements) {
                            //let [poshmarkId, brand] = await getPoshmark(element);
                            /*html_body = ` poshmark ${element}|${poshmarkId}|${brand}`;
                            send(html_body);
                            console.log(poshmarkId + " | " + brand);*/
                            console.log(elements);
                        }

                        resolve();
                    }
                );
            });
        });
    });
});

async function getPoshmark(element){

}