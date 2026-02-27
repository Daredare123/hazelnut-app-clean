importScripts('https://www.gstatic.com/firebasejs/8.10.1/firebase-app.js');
importScripts('https://www.gstatic.com/firebasejs/8.10.1/firebase-messaging.js');

firebase.initializeApp({
  apiKey: "AIzaSyDfhE0OubxHF-EEN_LkRm1Nm79gLraRM0Q",
  authDomain: "hazelnutarena-e2297.firebaseapp.com",
  projectId: "hazelnutarena-e2297",
  storageBucket: "hazelnutarena-e2297.firebasestorage.app",
  messagingSenderId: "279698258773",
  appId: "1:279698258773:web:010cdbdbbe201ecde78ddc"
});

const messaging = firebase.messaging();

messaging.onBackgroundMessage(function(payload) {
  console.log('[firebase-messaging-sw.js] Received background message ', payload);
  const notificationTitle = payload.notification?.title || payload.data?.title || 'Hazelnut Arena';
  const notificationOptions = {
    body: payload.notification?.body || payload.data?.body || '',
    icon: '/favicon.ico'
  };

  self.registration.showNotification(notificationTitle, notificationOptions);
});
