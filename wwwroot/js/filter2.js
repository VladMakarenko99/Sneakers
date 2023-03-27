const checkboxes = document.getElementsByClassName("checkbox");
const display = document.querySelector(".display");
const htmlUrl = document.getElementById("URL");

let url = "";
if (JSON.parse(localStorage.getItem('url')) !== null) {
    url = JSON.parse(localStorage.getItem('url'));
    console.log("YES!" + url);
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
for (let i of checkboxes) {
    i.addEventListener("change", function (el) {
        if(url.includes("&price="))
            brandString = url.split("&price=")[0];
        else if (url.includes("q:price=")) {
            url = url.replace("q:price=", "&price=");           
            brandString = url.split("&price=")[0];
        }
        else
            brandString = url;
        if (el.target.checked) {
            if (!brandString.includes("brand=")) {
                brandString += "/q:brand=" + el.target.value;
            }
            else {
                brandString += "+" + el.target.value;
            }
        }

        if (!el.target.checked) {
            brandString = brandString.replace("+" + el.target.value, "");
            brandString = brandString.replace(el.target.value, "");
            brandString = brandString.replace("/q:brand=+", "/q:brand=");
        }

        brandString = brandString.replace(/\/+/g, "/");

        if (brandString === "/q:brand=" || brandString === "q:" || brandString === "q:brand=") {
            brandString = "";
        }
        if (brandString === "" && url.includes("&price=")) {
            console.log("here");
            url = url.replace(/.+&price=/g, brandString + "/q:price=");
            htmlUrl.href = url;
            localStorage.setItem('url', JSON.stringify(url));
            return;
        }
        if (url.includes("&price=")) {
            url = url.replace(/.+&price=/g, brandString + "&price=");
            htmlUrl.href = url;
            localStorage.setItem('url', JSON.stringify(url));
            return;
        }
        if (url.includes("/q:price=")) {
            url = url.replace("/q:price=", brandString + "&price=");
        }
        else {
            url = brandString;
        }

        console.log(url);
        htmlUrl.href = url;
        localStorage.setItem('url', JSON.stringify(url));
        if (htmlUrl.href === "" || url === "") {
            htmlUrl.href = "/";
        }
    });
}

window.onload = function () {
    if (window.location.pathname === "/") {
        localStorage.clear();
        document.querySelectorAll('input[type="checkbox"]').forEach(function(checkbox) {
            checkbox.checked = false;
          });
        url = "";
      }      
      
      
    loadSliderValues();
    loadSortValue();
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
    if (!url.includes("brand=")) {
        url = "/q:" + priceString;
    }
    else {
        if (url.includes("&price=")) {
            url = url.replace(/\d+-\d+/g, min + "-" + max);
        }
        else {
            url += "&" + priceString;
        }
    }

    url = url.replace(/&+/g, "&");
    htmlUrl.href = url;
    localStorage.setItem('url', JSON.stringify(url));
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
    url = url.replace(/q:sort=.+/, "");
    url = url.replace(/&sort=.+/, "");
    let sortString = "";
    if(url.includes("q:sort=")){
        sortString = url.split("q:"[1])
    }
    if(url.includes("&sort=")){
        sortString = url.split("&"[1])
    }
    if (!sortString.includes("sort=")) {
        sortString = "sort=" + el.target.value;
    }
    else {
        sortString = url.replace(/sort=.+/, "sort=" + el.target.value);
    }
    if(sortString === "sort=default"){
        url = url.replace(/&sort=.+/, "");
        htmlUrl.href = url;
        return;
    }
    if (url === "" || url === "/" || url === null) {
        url = "/q:" + sortString;
    }
    else {
        url = url + "&" + sortString;
    }
    htmlUrl.href = url;
    localStorage.setItem('url', JSON.stringify(url));
    localStorage.setItem("sortValue", el.target.value);
});

function loadSortValue() {
    const sortValue = localStorage.getItem("sortValue");
    if (sortValue !== null) {
        document.getElementById("sort").value = sortValue;
    }
}