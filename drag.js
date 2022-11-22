window.addEventListener('DOMContentLoaded', () => {
    document.body.addEventListener('pointerdown', evt => {
        const { target } = evt;
        const appRegion = getComputedStyle(target)['-webkit-app-region'];

        if (appRegion === 'drag') {
            chrome.webview.hostObjects.sync.eventForwarder.MouseDownDrag();
            //evt.preventDefault();
            //evt.stopPropagation();
        }
    });
});