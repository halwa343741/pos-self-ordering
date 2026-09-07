// Cross-tab Synchronization via BroadcastChannel API
(function () {
    let channel = null;
    let dotNetHelper = null;

    window.posBroadcast = {
        init: function (dotNetRef) {
            dotNetHelper = dotNetRef;
            if (typeof BroadcastChannel !== 'undefined') {
                if (!channel) {
                    channel = new BroadcastChannel('pos_shared_bus');
                    channel.onmessage = function (event) {
                        if (dotNetHelper && event && event.data) {
                            dotNetHelper.invokeMethodAsync('OnBroadcastMessageReceived', JSON.stringify(event.data));
                        }
                    };
                }
            } else {
                console.warn('BroadcastChannel is not supported in this browser.');
            }
        },

        postMessage: function (type, payload) {
            if (channel) {
                try {
                    channel.postMessage({ type: type, payload: payload, timestamp: Date.now() });
                } catch (err) {
                    console.error('Error posting broadcast message:', err);
                }
            }
        }
    };
})();
