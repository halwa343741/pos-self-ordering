// PWA Helper and Browser Interop
window.pwaHelper = {
  registerServiceWorker: function () {
    if ('serviceWorker' in navigator) {
      window.addEventListener('load', () => {
        navigator.serviceWorker.register('/service-worker.js')
          .then(reg => console.log('PWA Service Worker registered:', reg.scope))
          .catch(err => console.warn('Service Worker registration failed:', err));
      });
    }
  },

  triggerVibrate: function (durationMs) {
    if ('vibrate' in navigator) {
      try {
        navigator.vibrate(durationMs || 50);
      } catch (e) {
        // Ignore vibration errors
      }
    }
  },

  isOnline: function () {
    return navigator.onLine;
  }
};

// Automatically register SW on load
window.pwaHelper.registerServiceWorker();
