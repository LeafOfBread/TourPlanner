window.onload = function () {
    var map = L.map('map', {
        center: [48.210033, 16.363449],
        zoom: 13
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    var marker = L.marker([51.5, -0.09]).addTo(map);

};