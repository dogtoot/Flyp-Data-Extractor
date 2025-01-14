const extensions = 'https://tools.joinflyp.com/my-items';

// A function to use as callback
function doStuffWithDom(domContent) {
    console.log('I received the following DOM content:\n' + domContent);
}

/*chrome.action.onClicked.addListener(async (tab) => {
    // ...check the URL of the active tab against our pattern and...
    if (tab.title.startsWith(extensions)) {
        // ...if it matches, send a message specifying a callback too
        chrome.tabs.sendMessage(tab.id, {text: 'report_back'}, doStuffWithDom);
    }
});*/

