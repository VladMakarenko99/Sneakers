window.onload = function () {
    const succesUrl = document.getElementById('succesUrl');
    let domain = window.location.hostname;
    let urlValue = "";
    let protocolValue = ""
    const protocol = window.location.protocol;
    switch (protocol) {
        case 'http:':
            protocolValue = 'http://';
            break;
        case 'https:':
            protocolValue = 'https://'
            break;
    }
    switch (domain) {
        case 'localhost':
            urlValue = protocolValue + 'localhost:7206/checkout/success/';
            break;
        default:
            urlValue = protocolValue + domain + '/checkout/success/';
            break;
    }
    succesUrl.value = urlValue;
}
