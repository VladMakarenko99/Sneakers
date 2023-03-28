window.onload = function () {
    if (window.location.pathname === "/") {
        localStorage.clear();
        document.querySelectorAll('input[type="checkbox"]').forEach(function (checkbox) {
            checkbox.checked = false;
        });
        url = "";
    }


    loadSliderValues();
    loadSortValue();
}

const checkboxes = document.getElementsByClassName("checkbox");
const display = document.querySelector(".display");
const htmlUrl = document.getElementById("URL");

let url = "";
if (localStorage.getItem("url")) {
    url = localStorage.getItem("url");
}
else {
    url = "";
}


let brandString = "";

for (const checkbox of checkboxes) {
    const value = localStorage.getItem(checkbox.value);
    if (value === 'checked') {
        checkbox.checked = true;
    }
}

// Save the checked state of each checkbox to localStorage when it is changed
for (const checkbox of checkboxes) {
    checkbox.addEventListener('change', (event) => {
        const value = event.target.value;
        const checked = event.target.checked;
        if (checked) {
            localStorage.setItem(value, 'checked');
        } else {
            localStorage.removeItem(value);
        }
    });
}

function findBrandString(inputString) {
    const regex = /(brand=[\w+-]+)/;
    const matches = inputString.match(regex);
    if (!matches) {
      return "";
    }
    const brandString = matches[1].replace("q:", "");
    const brands = brandString.split('=')[1].split('+');
    const validBrands = brands.filter((brand) =>
      ['Nike', 'Adidas', 'New-Balance'].includes(brand)
    );
    if (validBrands.length === 0) {
      return "";
    }
    const newBrandString = `brand=${validBrands.join('+')}`;
    return newBrandString;
  }
  
for (let i of checkboxes) {
    i.addEventListener("change", function (el) {
        if(url !== ""){
            brandString = findBrandString(url);
            console.log("FOUND" + brandString)
        }

        if (el.target.checked) {
            if (!brandString.includes("brand=")) {
                brandString += "brand=" + el.target.value;
            }
            else {
                brandString += "+" + el.target.value;
            }
        }

        if (!el.target.checked) {
            brandString = brandString.replace("+" + el.target.value, "");
            brandString = brandString.replace(el.target.value, "");
            brandString = brandString.replace("brand=+", "brand=");
        }

        brandString = brandString.replace(/\/+/g, "/");

        if (brandString === "brand=")
            brandString = "";


        if (url === "")
            url += "/q:" + brandString;

        if (!url.includes("brand=") && url !== "")
            url += "&" + brandString;

        if (url.includes("brand=")) {
            if (url.match(/brand=.+&/) && brandString !== "") {
                url = url.replace(/brand=.+&/, brandString + "&");
                htmlUrl.href = url;
                localStorage.setItem('url', url);
                return;
            }

            if (brandString === "") {
                url = url.replace(/&brand=.+&/, "&");
                if (url.match(/&brand=.+/))
                    url = url.replace(/&brand=.+/, "");
                if(url.match(/q:brand=/)){
                    url = url.replace(/q:brand=.+&/, "q:");
                    if(!url.includes("&"))
                        url = url.replace(/q:brand=.+/, "");
                }
                htmlUrl.href = url;
                localStorage.setItem('url', url);
                return;
            }
            url = url.replace(/brand=.+/, brandString);

        }

        if (url === "/q:" || url === "/")
            url = "";
        console.log(url);
        htmlUrl.href = url;
        localStorage.setItem('url', url);
    });
}

let sliderOne = document.getElementById("slider-1");
let sliderTwo = document.getElementById("slider-2");
let displayValOne = document.getElementById("range1");
let displayValTwo = document.getElementById("range2");
let minGap = 0;
let sliderTrack = document.querySelector(".slider-track");
let sliderMaxValue = document.getElementById("slider-1").max;

function slideOne() {
    if (parseInt(sliderTwo.value) - parseInt(sliderOne.value) <= minGap) {
        sliderOne.value = parseInt(sliderTwo.value) - minGap;
    }
    displayValOne.textContent = sliderOne.value;
    // fillColor();
}
function slideTwo() {
    if (parseInt(sliderTwo.value) - parseInt(sliderOne.value) <= minGap) {
        sliderTwo.value = parseInt(sliderOne.value) + minGap;
    }
    displayValTwo.textContent = sliderTwo.value;
    // fillColor();
}
// function fillColor() {
//     let percent1 = (sliderOne.value / sliderMaxValue) * 100;
//     let percent2 = (sliderTwo.value / sliderMaxValue) * 100;
//     if(localStorage.getItem("slider1Value") !== null && localStorage.getItem("slider2Value") !== null){
//         percent1 = (localStorage.getItem("slider1Value") / sliderMaxValue) * 100;
//         percent2 = (localStorage.getItem("slider2Value") / sliderMaxValue) * 100;
//     }

//     sliderTrack.style.background = `linear-gradient(to right, #dadae5 ${percent1}% , #3264fe ${percent1}% , #3264fe ${percent2}%, #dadae5 ${percent2}%)`;
// }

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
    if (url === "" || url === "/" || url === null)  {
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
    htmlUrl.href = url;
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
        htmlUrl.href = url;
        localStorage.setItem('url', url);
        return;
    }
    if (url === "" || url === "/" || url === null) {
        url = "/q:" + sortString;
    }
    else if (url.includes("&sort=ascending&") || url.includes("&sort=descending&")){
        url = url.replace(/&sort=\w+&/, "&" + sortString + "&");
        htmlUrl.href = url;
        localStorage.setItem('url', url);
        localStorage.setItem("sortValue", el.target.value);
        return;
    }
    else {
        url = url + "&" + sortString;
    }
    htmlUrl.href = url;
    localStorage.setItem('url', url);
    localStorage.setItem("sortValue", el.target.value);
});

function loadSortValue() {
    const sortValue = localStorage.getItem("sortValue");
    if (sortValue !== null) {
        document.getElementById("sort").value = sortValue;
    }
}