window.onload = function () {
    if (window.location.pathname === "/") {
        //localStorage.clear();
        document.querySelectorAll('input[type="checkbox"]').forEach(function(checkbox) {
            checkbox.checked = false;
          });
        url = "";
      }      
      
      
    // loadSliderValues();
    //loadSortValue();
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


const checkboxes = document.getElementsByClassName("checkbox");
const htmlUrl = document.getElementById("URL");

let url = "";
let stringArray = [];
if(localStorage.getItem("url")){
    url = localStorage.getItem("url");
    url = url.split("q:")[1];
    stringArray = url.split("&");
}

let brandString = "";
for (let i of checkboxes) {
    i.addEventListener("change", function (el) {
        console.log(el.target.value);
        
       if(stringArray.includes(/brand=.+/) || stringArray.includes(/brand=.+\+.+/gm)){
            console.log("brand is in the array");
            let index = stringArray.indexOf("brand=" + /.+/);
            console.log(index);
            brandString = stringArray[index];
            console.log(brandString);
       }
       else{

       }
    });
}


