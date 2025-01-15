var s = document.createElement('script');
s.src = chrome.runtime.getURL("injected_scripts/after.inject.js");
s.onload = function() {
	this.parentNode.removeChild(this);
};
(document.head||document.documentElement).appendChild(s);
