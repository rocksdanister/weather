//video headers
var videoClip = document.getElementById("demo_clip");
videoClip.addEventListener("timeupdate", myFunction);

var weatherHeader = document.getElementById("content-demo-header-weather-label");
var weatherIconSnow = document.getElementById("content-demo-header-weather-icon-snow");
var weatherIconRain = document.getElementById("content-demo-header-weather-icon-rain");
var weatherIconCloudy = document.getElementById("content-demo-header-weather-icon-cloudy");
var weatherPollen = document.getElementById("content-demo-header-stats-pollen");
var weatherWind = document.getElementById("content-demo-header-stats-wind");
var weatherHumidity = document.getElementById("content-demo-header-stats-humidity");
var weatherVisibility = document.getElementById("content-demo-header-stats-visibility");
var weatherPressure = document.getElementById("content-demo-header-stats-pressure");
var weatherDew = document.getElementById("content-demo-header-stats-dew");

function myFunction() {
  if (videoClip.currentTime >= 9.5) setWeatherHeaders("snow");
  else if (videoClip.currentTime >= 6.5) setWeatherHeaders("cloudy");
  else if (videoClip.currentTime >= 2.5)setWeatherHeaders("rain");
  else setWeatherHeaders("snow");
}

function setWeatherHeaders(weather)
{
  switch(weather) {
    case "snow":
      weatherHeader.innerHTML = "Snow";
      weatherPollen.innerHTML = "Low";
      weatherWind.innerHTML = "14 km/h";
      weatherHumidity.innerHTML = "50%";
      weatherVisibility.innerHTML = "25 km";
      weatherPressure.innerHTML = "1050 mb";
      weatherDew.innerHTML = "30°";

      weatherIconSnow.style.display = "block";
      weatherIconCloudy.style.display = "none";
      weatherIconRain.style.display = "none";
      break;
    case "rain":
      weatherHeader.innerHTML = "Rain";
      weatherPollen.innerHTML = "Medium";
      weatherWind.innerHTML = "12 km/h";
      weatherHumidity.innerHTML = "80%";
      weatherVisibility.innerHTML = "15 km";
      weatherPressure.innerHTML = "1050 mb";
      weatherDew.innerHTML = "60°";

      weatherIconSnow.style.display = "none";
      weatherIconCloudy.style.display = "none";
      weatherIconRain.style.display = "block";
      break;  
    case "cloudy":
      weatherHeader.innerHTML = "Cloudy";
      weatherPollen.innerHTML = "High";
      weatherWind.innerHTML = "9 km/h";
      weatherHumidity.innerHTML = "60%";
      weatherVisibility.innerHTML = "10 km";
      weatherPressure.innerHTML = "1050 mb";
      weatherDew.innerHTML = "60°";

      weatherIconSnow.style.display = "none";
      weatherIconCloudy.style.display = "block";
      weatherIconRain.style.display = "none";
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
  document.getElementById(id).scrollIntoView({ behavior: "smooth", block: "center", inline: "nearest" });
}
