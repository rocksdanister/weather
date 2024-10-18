//video headers
var videoClip = document.getElementById("demo-clip");
videoClip.addEventListener("timeupdate", videoFunction);

var headerTemp = document.getElementById("weather-header-temp");
var headerLabel = document.getElementById("weather-header-label");
var cardUvIndex = document.getElementById("card-uv-number");
var cardHumidity = document.getElementById("card-humidity");
var cardWind = document.getElementById("card-wind");
var cardAqi = document.getElementById("card-aqi-number");
var cardPressure = document.getElementById("card-pressure");

function videoFunction() {
  if (videoClip.currentTime >= 6.75) setWeatherHeaders("fog");
  else if (videoClip.currentTime >= 4) setWeatherHeaders("snow");
  else if (videoClip.currentTime >= 2.25) setWeatherHeaders("rain");
  else setWeatherHeaders("overcast");
}

function setWeatherHeaders(weather) {
  switch (weather) {
    case "fog":
      headerTemp.innerHTML = "3 °C";
      headerLabel.innerHTML = "Fog";
      cardUvIndex.innerHTML = "1";
      cardHumidity.innerHTML = "67%";
      cardWind.innerHTML = "6 kmh";
      cardAqi.innerHTML = "8";
      cardPressure.innerHTML = "1000 mb";
      break;
    case "snow":
      headerTemp.innerHTML = "4 °C";
      headerLabel.innerHTML = "Snow";
      cardUvIndex.innerHTML = "0";
      cardHumidity.innerHTML = "69%";
      cardWind.innerHTML = "16 kmh";
      cardAqi.innerHTML = "8";
      cardPressure.innerHTML = "1007 mb";
      break;
    case "rain":
      headerTemp.innerHTML = "12 °C";
      headerLabel.innerHTML = "Slight Rain";
      cardUvIndex.innerHTML = "0";
      cardHumidity.innerHTML = "65%";
      cardWind.innerHTML = "18 kmh";
      cardAqi.innerHTML = "20";
      cardPressure.innerHTML = "1015 mb";
      break;
    case "overcast":
      headerTemp.innerHTML = "15 °C";
      headerLabel.innerHTML = "Overcast";
      cardUvIndex.innerHTML = "1";
      cardHumidity.innerHTML = "64%";
      cardWind.innerHTML = "14 kmh";
      cardAqi.innerHTML = "15";
      cardPressure.innerHTML = "1001 mb";
      break;
  }
}

//threejs scene first run
document.addEventListener("sceneLoaded", () => {
  if (container.style.opacity == 0) setVisible(container);
  $(".indeterminate-progress-bar").css("display", "none");
  $(".item").each(function () {
    $(this).css("background-image", $(this).data("delayedsrc"));
  });
});

//helpers
async function setVisible(element) {
  for (let val = 0; val < 1; val += 0.1) {
    element.style.opacity = val;
    await new Promise((r) => setTimeout(r, 75));
  }
}

function hasClass(element, className) {
  return (" " + element.className + " ").indexOf(" " + className + " ") > -1;
}

function scrollToElement(id) {
  document
    .getElementById(id)
    .scrollIntoView({ behavior: "smooth", block: "center", inline: "nearest" });
}

//override badge size
window.onload = (e) => {
  document.querySelector('button.others').addEventListener('click', () => {
    document.querySelector('.downloads .drawer').classList.toggle('closed');
  })

  // const styleOverride = document.createElement("style");

  // // disable the animation and the box shadow
  // styleOverride.innerHTML = "div{height:fit-content;} div > img.large { width:250px; height:auto } img:hover {transform: translate(0, 0);cursor: pointer;box-shadow: none;}";
  // document
  //   .querySelector("ms-store-badge")
  //   .shadowRoot.appendChild(styleOverride);

  const navApp = navigator.appVersion ?? null;
  if (navApp == null) return;

  if (navApp.includes("Linux")) {
    // bring deb to front and put windows store in drawer
    const ms = document.querySelector('#ms-store').outerHTML;
    const deb = document.querySelector('#deb').outerHTML;
    document.querySelector('#mainDownload').innerHTML = deb;
    document.querySelector('.drawer').innerHTML = ms + document.querySelector('.drawer').innerHTML.replace(deb, '')
  }
};

