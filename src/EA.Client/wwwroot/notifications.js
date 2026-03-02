window.notificationInterop = {
    requestPermission: async function () {
        if (!('Notification' in window)) {
            return 'not-supported';
        }

        if (Notification.permission === 'granted') {
            return 'granted';
        }

        if (Notification.permission !== 'denied') {
            const result = await Notification.requestPermission();
            return result;
        }

        return Notification.permission;
    },

    scheduleNotification: function (title, body, delayMs) {
        if (!('Notification' in window)) {
            return false;
        }

        if (Notification.permission !== 'granted') {
            return false;
        }

        setTimeout(function () {
            new Notification(title, {
                body: body,
                icon: '/favicon.ico'
            });
        }, delayMs);

        return true;
    }
};
