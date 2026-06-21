// Smile Desk — SVG icon system
// Replaces every <i class="ti ti-xxx"></i> tag with an inline SVG icon.
// No external font/CDN dependency — icons render instantly and reliably,
// even with no internet connection, since the SVG paths live in this file.

(function () {
    var ICONS = {
        "alert-triangle": '<path d="M12 9v4"/><path d="M10.4 3.9 2.5 17a2 2 0 0 0 1.7 3h15.6a2 2 0 0 0 1.7-3L13.6 3.9a2 2 0 0 0-3.2 0z"/><path d="M12 17h.01"/>',
        "arrow-left": '<path d="M19 12H5"/><path d="m12 19-7-7 7-7"/>',
        "arrow-right": '<path d="M5 12h14"/><path d="m12 5 7 7-7 7"/>',
        "arrow-right-circle": '<circle cx="12" cy="12" r="10"/><path d="m10 8 4 4-4 4"/>',
        "bell": '<path d="M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9"/><path d="M10.3 21a1.94 1.94 0 0 0 3.4 0"/>',
        "bell-ringing": '<path d="M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9"/><path d="M10.3 21a1.94 1.94 0 0 0 3.4 0"/><path d="M4 4l-1 1"/><path d="M20 4l1 1"/>',
        "bolt": '<path d="M13 2 3 14h9l-1 8 10-12h-9z"/>',
        "book": '<path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/>',
        "bowl": '<path d="M3 11h18"/><path d="M3 11a9 7 0 0 0 18 0"/><path d="M5 11V8a7 4 0 0 1 14 0v3"/>',
        "box-off": '<path d="M3 7l9 5 9-5"/><path d="M12 22V12"/><path d="M3 7v10l9 5 9-5V7"/><path d="m3 3 18 18" stroke-opacity=".5"/>',
        "brand-instagram": '<rect x="3" y="3" width="18" height="18" rx="5"/><circle cx="12" cy="12" r="3.5"/><circle cx="17.2" cy="6.8" r="1"/>',
        "brand-linkedin": '<rect x="3" y="3" width="18" height="18" rx="2"/><path d="M8.5 10v6"/><path d="M8.5 7.5h.01"/><path d="M12.5 16v-3.5a2 2 0 0 1 4 0V16"/><path d="M12.5 10v6"/>',
        "brand-x": '<path d="M4 4l16 16"/><path d="M20 4 4 20"/>',
        "building-bank": '<path d="M3 21h18"/><path d="M5 21V9l7-5 7 5v12"/><path d="M9 21v-6h6v6"/><path d="M5 9h14"/>',
        "building-community": '<path d="M3 21V9l5-4 5 4v12"/><path d="M13 21v-9l4-3 4 3v9"/><path d="M7 14h2"/><path d="M7 17h2"/>',
        "calendar": '<rect x="3" y="5" width="18" height="16" rx="2"/><path d="M3 10h18"/><path d="M8 3v4"/><path d="M16 3v4"/>',
        "calendar-event": '<rect x="3" y="5" width="18" height="16" rx="2"/><path d="M3 10h18"/><path d="M8 3v4"/><path d="M16 3v4"/><circle cx="12" cy="15" r="2"/>',
        "calendar-off": '<rect x="3" y="5" width="18" height="16" rx="2"/><path d="M3 10h18"/><path d="M8 3v4"/><path d="M16 3v4"/><path d="m5 7 14 14" stroke-opacity=".6"/>',
        "calendar-plus": '<rect x="3" y="5" width="18" height="16" rx="2"/><path d="M3 10h18"/><path d="M8 3v4"/><path d="M16 3v4"/><path d="M12 14v6"/><path d="M9 17h6"/>',
        "check": '<path d="M5 12l5 5L20 7"/>',
        "circle-check": '<circle cx="12" cy="12" r="9"/><path d="m8 12 3 3 5-6"/>',
        "circle-check-filled": '<circle cx="12" cy="12" r="9" fill="currentColor"/><path d="m8 12 3 3 5-6" stroke="white"/>',
        "clock": '<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 3"/>',
        "clock-hour-4": '<circle cx="12" cy="12" r="9"/><path d="M12 7v5l4 2"/>',
        "code": '<path d="m9 7-5 5 5 5"/><path d="m15 7 5 5-5 5"/>',
        "coin": '<circle cx="12" cy="12" r="9"/><path d="M9 10a3 2 0 1 0 6 0a3 2 0 1 0 -6 0"/><path d="M12 7v2"/><path d="M12 15v2"/>',
        "compass": '<circle cx="12" cy="12" r="9"/><path d="m15 9-2 6-6 2 2-6z"/>',
        "credit-card": '<rect x="2" y="5" width="20" height="14" rx="2"/><path d="M2 10h20"/>',
        "currency-rupee": '<path d="M6 4h11"/><path d="M6 8h11"/><path d="M9 4a4 4 0 0 1 0 8H6l8 8"/>',
        "device-floppy": '<path d="M5 4h11l3 3v12a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V5a1 1 0 0 1 1-1z"/><path d="M9 4v4h6V4"/><path d="M9 20v-5h6v5"/>',
        "device-laptop": '<rect x="3" y="4" width="18" height="12" rx="1"/><path d="M2 19h20"/>',
        "edit": '<path d="M12 20h9"/><path d="M16.5 3.5a2 2 0 0 1 3 3L8 18l-4 1 1-4z"/>',
        "file-description": '<path d="M14 3v5h5"/><path d="M6 3h8l5 5v12a1 1 0 0 1-1 1H6a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1z"/><path d="M9 13h6"/><path d="M9 17h6"/>',
        "first-aid-kit": '<rect x="3" y="7" width="18" height="13" rx="2"/><path d="M9 7V5a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2v2"/><path d="M12 11v5"/><path d="M9.5 13.5h5"/>',
        "hand-finger": '<path d="M8 13V6a1.5 1.5 0 0 1 3 0v6"/><path d="M11 12V5a1.5 1.5 0 0 1 3 0v7"/><path d="M14 12.5V6a1.5 1.5 0 0 1 3 0v9"/><path d="M8 13a4 4 0 0 0 4 6h3a4 4 0 0 0 4-4v-3"/>',
        "hand-heart": '<path d="M11 14h-1a4 4 0 0 1 0-8 4 4 0 0 1 2.2.7"/><path d="M3 17l3-1 3 1 3-1 3 1 3-1 3 1"/><path d="M12.5 4.5a2 2 0 0 1 3.5 1.3c0 1.2-1.4 2.4-3.5 4.2-2.1-1.8-3.5-3-3.5-4.2a2 2 0 0 1 3.5-1.3z" fill="currentColor"/>',
        "hand-stop": '<path d="M8 12V6a1.5 1.5 0 0 1 3 0v5"/><path d="M11 11V5a1.5 1.5 0 0 1 3 0v6"/><path d="M14 11.5V7a1.5 1.5 0 0 1 3 0v7.5"/><path d="M8 13a4 4 0 0 0 4 7h2a4 4 0 0 0 4-4v-2"/><path d="M5 13l1-1"/>',
        "heart": '<path d="M12 20s-7-4.4-9.5-9A5.5 5.5 0 0 1 12 5.5 5.5 5.5 0 0 1 21.5 11c-2.5 4.6-9.5 9-9.5 9z"/>',
        "heart-handshake": '<path d="M12 7s-1.5-2-3.5-2A3.5 3.5 0 0 0 5 8.5C5 12 9 15 12 17"/><path d="M12 7s1.5-2 3.5-2A3.5 3.5 0 0 1 19 8.5c0 1-.3 1.9-.8 2.7"/><path d="M8 13l2 2 2-2 2 2 2-2"/>',
        "home": '<path d="M4 11.5 12 4l8 7.5"/><path d="M6 10v10h12V10"/><path d="M10 20v-6h4v6"/>',
        "info-circle": '<circle cx="12" cy="12" r="9"/><path d="M12 8h.01"/><path d="M11 12h1v5h1"/>',
        "key": '<circle cx="8" cy="15" r="4"/><path d="M10.85 12.15 19 4"/><path d="M16 7l2 2"/><path d="M13 10l2 2"/>',
        "layout-dashboard": '<rect x="3" y="3" width="8" height="9" rx="1"/><rect x="13" y="3" width="8" height="5" rx="1"/><rect x="13" y="10" width="8" height="11" rx="1"/><rect x="3" y="14" width="8" height="7" rx="1"/>',
        "list-check": '<path d="M5 7h1"/><path d="M5 12h1"/><path d="M5 17h1"/><path d="M9 7h10"/><path d="M9 12h10"/><path d="m9 17 2 2 4-4"/>',
        "list-details": '<path d="M5 7h1"/><path d="M5 12h1"/><path d="M5 17h1"/><path d="M9 7h10"/><path d="M9 12h6"/><path d="M9 17h8"/>',
        "lock": '<rect x="5" y="11" width="14" height="9" rx="2"/><path d="M8 11V7a4 4 0 0 1 8 0v4"/>',
        "lock-square": '<rect x="3" y="3" width="18" height="18" rx="2"/><rect x="8" y="12" width="8" height="6" rx="1"/><path d="M10 12V9a2 2 0 0 1 4 0v3"/>',
        "login-2": '<path d="M10 17v1a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2h-6a2 2 0 0 0-2 2v1"/><path d="M3 12h12"/><path d="m11 8 4 4-4 4"/>',
        "logout-2": '<path d="M14 7V6a2 2 0 0 0-2-2H6a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2v-1"/><path d="M21 12H9"/><path d="m17 16 4-4-4-4"/>',
        "mail": '<rect x="3" y="5" width="18" height="14" rx="2"/><path d="m3 7 9 6 9-6"/>',
        "mail-forward": '<rect x="3" y="6" width="13" height="12" rx="2"/><path d="m3 8 6.5 5L16 8"/><path d="M17 13h4"/><path d="m18.5 11 2.5 2-2.5 2"/>',
        "map-pin": '<path d="M12 21s-7-6.2-7-11.5A7 7 0 0 1 19 9.5C19 14.8 12 21 12 21z"/><circle cx="12" cy="9.5" r="2.5"/>',
        "package": '<path d="M12 3 3 7.5v9L12 21l9-4.5v-9z"/><path d="M3 7.5 12 12l9-4.5"/><path d="M12 21v-9"/>',
        "phone": '<path d="M5 4h4l2 5-2.5 1.5a11 11 0 0 0 5 5L15 13l5 2v4a2 2 0 0 1-2 2A16 16 0 0 1 3 6a2 2 0 0 1 2-2z"/>',
        "plus": '<path d="M12 5v14"/><path d="M5 12h14"/>',
        "point-filled": '<circle cx="12" cy="12" r="5" fill="currentColor"/>',
        "receipt": '<path d="M5 3h14v18l-2.5-1.5L14 21l-2-1.5L10 21l-2.5-1.5L5 21z"/><path d="M8 7h8"/><path d="M8 11h8"/><path d="M8 15h5"/>',
        "rocket": '<path d="M5 16c-1 1-1.5 4-1 5 1-.5 4-1 5-2"/><path d="M9 13c-3 1-5 4-5 4l3 3s3-2 4-5"/><path d="M11 13c2 3 6 1 8-1 3-3 4-9 4-9s-6 1-9 4c-2 2-3.5 4-3 6z"/><circle cx="15" cy="9" r="1.5" fill="currentColor"/>',
        "route": '<circle cx="6" cy="19" r="2"/><circle cx="18" cy="5" r="2"/><path d="M8 19h7a4 4 0 0 0 4-4 4 4 0 0 0-4-4H9a4 4 0 0 1-4-4 4 4 0 0 1 4-4h7"/>',
        "search": '<circle cx="11" cy="11" r="7"/><path d="m21 21-4.3-4.3"/>',
        "send": '<path d="M5 12 21 4l-7 17-3-7-7-2z"/>',
        "shield-check": '<path d="M12 3 4 6v6c0 5 4 8 8 9 4-1 8-4 8-9V6z"/><path d="m9 12 2.5 2.5L15.5 10"/>',
        "shield-lock": '<path d="M12 3 4 6v6c0 5 4 8 8 9 4-1 8-4 8-9V6z"/><rect x="9.5" y="11" width="5" height="4" rx="1"/><path d="M10.5 11V9.5a1.5 1.5 0 0 1 3 0V11"/>',
        "shirt": '<path d="M8 4 4 7l2 3 2-1.5V20h8V8.5L18 10l2-3-4-3-2 2h-4z"/>',
        "sofa": '<path d="M4 12V8a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v4"/><path d="M3 12h18v5a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1z"/><path d="M5 18v2"/><path d="M19 18v2"/>',
        "sparkles": '<path d="M12 3 13.5 8 19 9.5 13.5 11 12 16.5 10.5 11 5 9.5 10.5 8z"/><path d="M19 17l.7 1.8L21.5 19.5l-1.8.7-.7 1.8-.7-1.8-1.8-.7 1.8-.7z"/>',
        "target-arrow": '<circle cx="12" cy="12" r="9"/><circle cx="12" cy="12" r="4.5"/><path d="m20 4-7 7"/><path d="M20 4h-4"/><path d="M20 4v4"/>',
        "trash": '<path d="M4 7h16"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M6 7l1 13a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1l1-13"/><path d="M9 7V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v3"/>',
        "truck-delivery": '<rect x="2" y="8" width="11" height="8" rx="1"/><path d="M13 11h3l3 3v2h-6z"/><circle cx="6" cy="18" r="1.5"/><circle cx="17" cy="18" r="1.5"/>',
        "upload": '<path d="M12 16V4"/><path d="m7 8 5-5 5 5"/><path d="M5 16v3a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1v-3"/>',
        "user": '<circle cx="12" cy="8" r="4"/><path d="M5 21v-2a6 6 0 0 1 14 0v2"/>',
        "user-edit": '<path d="M9.5 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8z"/><path d="M4 21v-2a5.5 5.5 0 0 1 8-4.9"/><path d="M17 16l-3.5 3.5-1 2.5 2.5-1z"/>',
        "user-plus": '<circle cx="9" cy="8" r="4"/><path d="M3 21v-2a6 6 0 0 1 9-5.2"/><path d="M17 9v5"/><path d="M14.5 11.5h5"/>',
        "users": '<circle cx="8" cy="8" r="3.5"/><path d="M2 20v-1a5 5 0 0 1 5-5h2a5 5 0 0 1 5 5v1"/><circle cx="17" cy="8.5" r="3"/><path d="M16 13.3a5 5 0 0 1 6 4.7v2"/>',
        "x": '<path d="M5 5l14 14"/><path d="M19 5 5 19"/>'
    };

    function buildSvg(name) {
        var body = ICONS[name];
        if (!body) return null;
        return '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">' + body + '</svg>';
    }

    function renderIcons(root) {
        var nodes = (root || document).querySelectorAll('i.ti');
        nodes.forEach(function (el) {
            var cls = Array.prototype.find.call(el.classList, function (c) {
                return c.indexOf('ti-') === 0;
            });
            if (!cls) return;
            var name = cls.slice(3);
            var svg = buildSvg(name);
            if (!svg) return;
            el.innerHTML = svg;
            el.classList.add('sd-icon');
            el.style.display = 'inline-flex';
            el.style.verticalAlign = 'middle';
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        renderIcons(document);
    });

    // Expose globally in case other inline scripts inject new icon markup later
    window.SmileDeskIcons = { render: renderIcons };
})();
