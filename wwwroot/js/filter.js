let url = localStorage.getItem('url') || '';
window.onload = function () {
    if (window.location.pathname === "/") {
        localStorage.clear();
        document.querySelectorAll('input[type="checkbox"]').forEach(function (checkbox) {
            checkbox.checked = false;
        });
        url = "";
    }
    let div = document.querySelector('.product-grid');
    if(window.location.pathname.includes('q:') && div !== null){
        let elements = div.querySelectorAll('.product'); 
        let count = elements.length + 4;
        const load = document.getElementById("load-more");
        if(window.location.pathname.includes('/load=') && load !== null)
            load.href = window.location.pathname.replace(/\/load=\d+/gm, '/load=' + count);
        else if (!window.location.pathname.includes('/load=') && load !== null)
            load.href = window.location.pathname + '/load=' + count;
    }
    let scrollTop = localStorage.getItem('scrollPosition');
    if (scrollTop !== null && window.location.pathname.includes("load=")) {
        let num = Number(scrollTop);
        window.scrollTo(0, num);
    }
    loadSliderValues();
    loadSortValue();
    loadSearchValue();
    if(localStorage.getItem("shownFilter"))
        document.querySelector(".filter-to-show").classList.toggle("show");
}

const brandCheckboxes = document.querySelectorAll('.brand-checkbox');

function getCheckedBrandValue(value) {
    return localStorage.getItem(value) === 'checked';
}

function setCheckedBrandValue(value, checked) {
    if (checked) {
        localStorage.setItem(value, 'checked');
    } else {
        localStorage.removeItem(value);
    }
}

function findBrandString(inputString) {
    const regex = /(brand=[\w*-]+)/;
    const matches = inputString.match(regex);
    if (!matches) {
        return '';
    }
    const brandString = matches[1].replace('q:', '');
    const brands = brandString.split('=')[1].split('*');
    const validBrands = brands.filter((brand) =>
        ['Nike', 'Adidas', 'New-Balance'].includes(brand)
    );
    if (validBrands.length === 0) {
        return '';
    }
    return `brand=${validBrands.join('*')}`;
}

function updateUrlWithBrandString(checkbox) {
    console.log("ENTER BRAND");

    let brandString = '';
    if (url !== '') {
        brandString = findBrandString(url);
    }

    if (checkbox.checked) {
        if (!brandString.includes('brand=')) {
            brandString += 'brand=' + checkbox.value;
        } else if (!brandString.includes(checkbox.value) && brandString.includes('brand=')) {
            console.log(brandString + " add");
            brandString += '*' + checkbox.value;
        }
    } else {
        brandString = brandString
            .replace('*' + checkbox.value, '')
            .replace(checkbox.value, '')
            .replace('brand=*', 'brand=');
        console.log(brandString + " return");
    }
    brandString = brandString.replace(/\/+/g, '/');

    if (brandString === 'brand=') {
        brandString = '';
    }

    if (url === '') {
        url += '/q:' + brandString;
    } else if (!url.includes('brand=') && brandString !== '') {
        url += '&' + brandString;
    } else if (url.includes('brand=')) {
        if (url.match(/brand=[^&]*&?/gm) && brandString !== '') {
            console.log('WORK1');
            url = url.replace(/brand=[^&]*&?/gm, brandString + '&');
            if (url.match(/&$/))
                url = url.replace(/&$/, '');
        } else if (brandString === '') {
            url = url
                .replace(/&brand=.+&/, '&')
                .replace(/&brand=.+/, '')
                .replace(/q:brand=.+&/, 'q:')
                .replace(/q:brand=.+/, '');
        } else {
            url = url.replace(/brand=\w+/, brandString);
        }
    }

    if (url === '/q:' || url === '/') {
        url = '';
    }

    console.log(url);
    localStorage.setItem('url', url);
    return url;
}

brandCheckboxes.forEach((checkbox) => {
    const value = checkbox.value;
    checkbox.checked = getCheckedBrandValue(value);
    checkbox.addEventListener('change', () => {
        setCheckedBrandValue(value, checkbox.checked);
        if (updateUrlWithBrandString(checkbox) === '') {
            HandleUrl("/");
            return;
        }
        HandleUrl(updateUrlWithBrandString(checkbox));
    });
});


const categoryCheckboxes = document.querySelectorAll('.category-checkbox');


function getCheckedCategoryValue(value) {
    return localStorage.getItem(value) === 'checked';
}

function setCheckedCategoryValue(value, checked) {
    if (checked) {
        localStorage.setItem(value, 'checked');
    } else {
        localStorage.removeItem(value);
    }
}

function findCategoryString(inputString) {
    const regex = /(category=[\w*-]+)/;
    const matches = inputString.match(regex);
    if (!matches) {
        return '';
    }
    const brandString = matches[1].replace('q:', '');
    const brands = brandString.split('=')[1].split('*');
    const validBrands = brands.filter((brand) =>
        ['Everyday', 'Basketball', 'Running', 'Skating', 'Football', 'Handball'].includes(brand)
    );
    if (validBrands.length === 0) {
        return '';
    }

    console.log(`category=${validBrands.join('*')}`);
    return `category=${validBrands.join('*')}`;
}

function updateUrlWithCategoryString(checkbox) {
    console.log("ENTER category");
    let brandString = '';
    if (url !== '') {
        brandString = findCategoryString(url);
    }

    if (checkbox.checked) {
        if (!brandString.includes('category=')) {
            brandString += 'category=' + checkbox.value;
        } else if (!brandString.includes(checkbox.value) && brandString.includes('category=')) {
            console.log(brandString + " add");
            brandString += '*' + checkbox.value;
        }
    } else {
        brandString = brandString
            .replace('*' + checkbox.value, '')
            .replace(checkbox.value, '')
            .replace('category=*', 'category=');
        console.log(brandString + " return");
    }
    brandString = brandString.replace(/\/+/g, '/');

    if (brandString === 'category=') {
        brandString = '';
    }

    if (url === '') {
        url += '/q:' + brandString;
    } else if (!url.includes('category=') && brandString !== '') {
        url += '&' + brandString;
    } else if (url.includes('category=')) {
        if (url.match(/category=[^&]*&?/gm) && brandString !== '') {
            console.log('WORK1');
            url = url.replace(/category=[^&]*&?/gm, brandString + '&');
            if (url.match(/&$/))
                url = url.replace(/&$/, '');
        } else if (brandString === '') {
            url = url
                .replace(/&category=.+&/, '&')
                .replace(/&category=.+/, '')
                .replace(/q:category=.+&/, 'q:')
                .replace(/q:category=.+/, '');
        } else {
            url = url.replace(/category=\w+/, brandString);
        }
    }

    if (url === '/q:' || url === '/') {
        url = '';
    }

    console.log(url);
    localStorage.setItem('url', url);
    return url;
}

categoryCheckboxes.forEach((checkbox) => {
    const value = checkbox.value;
    checkbox.checked = getCheckedCategoryValue(value);
    checkbox.addEventListener('change', () => {
        setCheckedCategoryValue(value, checkbox.checked);
        if (updateUrlWithCategoryString(checkbox) === '') {
            HandleUrl('/');
            return;
        }
        HandleUrl(updateUrlWithCategoryString(checkbox)); 
    });
});





let sliderOne = document.getElementById("slider-1");
let sliderTwo = document.getElementById("slider-2");
let displayValOne = document.getElementById("range1");
let displayValTwo = document.getElementById("range2");
let minGap = 0;
let sliderTrack = document.querySelector(".slider-track");
let sliderMaxValue = document.getElementById("slider-1").max;

sliderOne.addEventListener("change", formPriceString);
sliderTwo.addEventListener("change", formPriceString);
function formPriceString() {
    let priceString = "";
    let min = sliderOne.value;
    let max = sliderTwo.value;
    if (priceString.includes("price=")) {
        priceString = priceString.replace(/\d+/g, min + "-" + max)
    }
    else {
        priceString = "price=" + min + "-" + max;
    }
    if (url === "" || url === "/" || url === null) {
        url = "/q:" + priceString;
    }
    if (!url.includes("price=") && url !== "") {
        url += "&" + priceString;
    }
    if (url.match(/q:price=/))
        url = url.replace(/q:price=.+\d+/, "q:" + priceString);
    if (url.match(/&price=.+\d+/))
        url = url.replace(/&price=.+\d+/, "&" + priceString);

    url = url.replace(/&+/g, "&");

    HandleUrl(url);
    localStorage.setItem('url', url);
    localStorage.setItem("slider1Value", min);
    localStorage.setItem("slider2Value", max);
}

function loadSliderValues() {
    const slider1Value = localStorage.getItem("slider1Value");
    const slider2Value = localStorage.getItem("slider2Value");

    if (slider1Value !== null) {
        document.getElementById("slider-1").value = slider1Value;
        document.getElementById("range1").textContent = slider1Value;
    }

    if (slider2Value !== null) {
        document.getElementById("slider-2").value = slider2Value;
        document.getElementById("range2").textContent = slider2Value;
    }
}

const sorter = document.getElementById("sort");
sorter.addEventListener("change", function (el) {
    let sortString = "";
    if (localStorage.getItem("sortValue")) {
        sortString = "sort=" + localStorage.getItem("sortValue")
    }
    if (url.includes("q:sort=")) {
        sortString = url.split("q:"[1])
    }
    if (url.includes("&sort=")) {
        sortString = url.split("&"[1])
    }
    if (!sortString.includes("sort=")) {
        sortString = "sort=" + el.target.value;
    }
    else {
        sortString = url.replace(/sort=.+/, "sort=" + el.target.value);
    }
    if (sortString === "sort=default") {
        localStorage.removeItem("sortValue");
        if (url.match(/&sort=\w+&/)) {
            url = url.replace(/&sort=\w+&/, "&");
        }
        if (url.match(/q:sort=\w+&/)) {
            url = url.replace(/q:sort=\w+&/, "q:");
        }
        if (url.match(/&sort=\w+/)) {
            url = url.replace(/&sort=\w+/, "");
        }
        if (url.match(/q:sort=\w+$/))
            url = url.replace(/q:sort=\w+$/, "");
        HandleUrl(url);
        localStorage.setItem('url', url);
        return;
    }
    if (url === "" || url === "/" || url === null) {
        url = "/q:" + sortString;
    }
    else if (url.includes("&sort=ascending&") || url.includes("&sort=descending&")) {
        url = url.replace(/&sort=\w+&/, "&" + sortString + "&")
        HandleUrl(url);
        localStorage.setItem('url', url);
        localStorage.setItem("sortValue", el.target.value);
        return;
    }
    else if (url.includes("&sort=")) {
        url = url.replace(/&sort=\w+$/gm, "&" + sortString);
        HandleUrl(url);
        localStorage.setItem('url', url);
        localStorage.setItem("sortValue", el.target.value);
        return;
    }
    else if (url.includes("q:sort=")) {
        url = url.replace(/q:sort=\w+/gm, "q:" + sortString);
        HandleUrl(url);
        localStorage.setItem('url', url);
        localStorage.setItem("sortValue", el.target.value);
        return;
    }
    else {
        url = url + "&" + sortString;
    }
    HandleUrl(url);
    localStorage.setItem('url', url);
    localStorage.setItem("sortValue", el.target.value);
});

function loadSortValue() {
    const sortValue = localStorage.getItem("sortValue");
    if (sortValue !== null) {
        document.getElementById("sort").value = sortValue;
    }
}

const search = document.getElementById("search");
search.addEventListener("keyup", function (el) {
    const searchValue = el.target.value;
    if (searchValue === "") {
        search.value = "";
        localStorage.removeItem("searchValue");
        search.focus();
        history.pushState(null, null, '/');
        return;
    }
    localStorage.setItem("searchValue", searchValue);
    const newUrl = "/q:search=" + searchValue;
    history.pushState(null, null, newUrl.replace(/\s/g, '*'));
});

search.addEventListener('keydown', (event) => {
    if (event.key === 'Enter' && document.activeElement === search) {
        localStorage.clear();
        localStorage.setItem("searchValue", search.value)
        window.location.reload();
    }
});

function loadSearchValue() {
    const searchValue = localStorage.getItem("searchValue");
    if (searchValue !== null) {
        document.getElementById("search").value = searchValue;
        search.focus();
    }
}
function HandleUrl(url){
    if(window.innerWidth < 666){
        console.log('if')
        document.getElementById('confirm').href = url;
    }
    else{
        console.log('else')
        window.location.href = url;
    }
}