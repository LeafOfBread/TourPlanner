let map;
let routeLayer = null;

window.onload = function () {
    map = L.map('map').setView([48.2082, 16.3738], 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19
    }).addTo(map);
};

function showRoute(fromLat, fromLng, toLat, toLng) {
    console.log("From:", fromLat, fromLng, "To:", toLat, toLng);
    if (!map) {
        console.warn("Map is not initialized yet.");
        return;
    }

    if (routeLayer) {
        map.removeLayer(routeLayer);
    }

    routeLayer = L.polyline([
        [fromLat, fromLng],
        [toLat, toLng]
    ], { color: 'blue' }).addTo(map);

    map.fitBounds(routeLayer.getBounds());
}
