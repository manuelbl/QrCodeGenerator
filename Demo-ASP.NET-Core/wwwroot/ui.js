(function () {
    var qrCodeImage;
    var textField;
    var borderField;
    var eccSelect;

    function updateQrCode() {
        var url = new URL('qrcode/svg', document.baseURI);
        url.searchParams.append('text', textField.value);
        url.searchParams.append('ecc', eccSelect.value);
        url.searchParams.append('border', borderField.value);
        qrCodeImage.src = url;
    }

    function init() {
        textField = document.getElementById('text');
        qrCodeImage = document.getElementById('qrcode');
        borderField = document.getElementById('border');
        eccSelect = document.getElementById('ecc');

        textField.onchange = function () { updateQrCode(); }
        textField.oninput = function () { updateQrCode(); }
        borderField.onchange = function () { updateQrCode(); }
        borderField.oninput = function () { updateQrCode(); }
        eccSelect.onchange = function () { updateQrCode(); }
    }

    init();
})();
