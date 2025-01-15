var s = document.createElement('script');
s.src = chrome.runtime.getURL("injected_scripts/before.inject.js");
s.onload = function() {
	this.parentNode.removeChild(this);
};
(document.head||document.documentElement).insertBefore(s,(document.head||document.documentElement).firstChild);
